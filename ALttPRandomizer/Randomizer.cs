namespace ALttPRandomizer {
    using ALttPRandomizer.Azure;
    using ALttPRandomizer.Model;
    using ALttPRandomizer.Options;
    using ALttPRandomizer.Settings;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Text.Json;
    using System.Threading.Tasks;

    public class Randomizer {
        public Randomizer(
                AzureStorage azureStorage,
                CommonSettingsProcessor settingsProcessor,
                IOptionsMonitor<ServiceOptions> optionsMonitor,
                ILogger<Randomizer> logger) {
            this.AzureStorage = azureStorage;
            this.SettingsProcessor = settingsProcessor;
            this.OptionsMonitor = optionsMonitor;
            this.Logger = logger;
        }

        private CommonSettingsProcessor SettingsProcessor { get; }
        private AzureStorage AzureStorage { get; }
        private IOptionsMonitor<ServiceOptions> OptionsMonitor { get; }
        private ILogger<Randomizer> Logger { get; }
        private ServiceOptions Configuration => this.OptionsMonitor.CurrentValue;

        public void Randomize(string id, SeedSettings settings) {
            var start = new ProcessStartInfo() {
                FileName = Configuration.PythonPath,
                WorkingDirectory = Configuration.RandomizerPath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };

            var args = start.ArgumentList;
            args.Add("DungeonRandomizer.py");
            args.Add("--rom");
            args.Add(this.Configuration.Baserom);
            args.Add("--bps");

            args.Add("--outputpath");
            args.Add(Path.GetTempPath());

            args.Add("--outputname");
            args.Add(id);

            args.Add("--spoiler=json");

            args.Add("--reduce_flashing");
            args.Add("--quickswap");

            this.AddArgs(args, this.SettingsProcessor.GetSettingPair(settings.Mode));
            this.AddArgs(args, this.SettingsProcessor.GetSettingPair(settings.Weapons));
            this.AddArgs(args, this.SettingsProcessor.GetSettingPair(settings.Goal));

            this.AddArgs(args, this.SettingsProcessor.GetSettingPair(nameof(SeedSettings.SmallKeys), settings.SmallKeys));
            this.AddArgs(args, this.SettingsProcessor.GetSettingPair(nameof(SeedSettings.BigKeys), settings.BigKeys));
            this.AddArgs(args, this.SettingsProcessor.GetSettingPair(nameof(SeedSettings.Maps), settings.Maps));
            this.AddArgs(args, this.SettingsProcessor.GetSettingPair(nameof(SeedSettings.Compasses), settings.Compasses));

            this.AddArgs(args, this.SettingsProcessor.GetSettingPair(settings.EntranceShuffle));
            if (settings.EntranceShuffle != EntranceShuffle.Vanilla) {
                args.Add("--shufflelinks");
                args.Add("--shuffletavern");
            }
            this.AddArgs(args, this.SettingsProcessor.GetSettingPair(settings.SkullWoods));
            this.AddArgs(args, this.SettingsProcessor.GetSettingPair(settings.LinkedDrops));

            this.AddArgs(args, this.SettingsProcessor.GetSettingPair(settings.BossShuffle));
            this.AddArgs(args, this.SettingsProcessor.GetSettingPair(settings.EnemyShuffle));

            this.AddArgs(args, this.SettingsProcessor.GetSettingPair(settings.ShopShuffle));
            this.AddArgs(args, this.SettingsProcessor.GetSettingPair(settings.DropShuffle));
            this.AddArgs(args, this.SettingsProcessor.GetSettingPair(settings.Pottery));
            if (settings.Pottery != Pottery.Vanilla && settings.Pottery != Pottery.Lottery) {
                args.Add("--colorizepots");
            }

            this.Logger.LogInformation("Randomizing with args: {args}", string.Join(" ", args));

            var process = Process.Start(start) ?? throw new GenerationFailedException("Process failed to start.");
            process.EnableRaisingEvents = true;

            process.OutputDataReceived += (_, args) => this.Logger.LogInformation("Randomizer STDOUT: {output}", args.Data);
            process.ErrorDataReceived += (_, args) => this.Logger.LogInformation("Randomizer STDERR: {output}", args.Data);

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            process.Exited += async (sender, args) => {
                var exitcode = process.ExitCode;

                if (exitcode != 0) {
                    this.GenerationFailed(id, exitcode);
                } else {
                    await this.GenerationSucceeded(id, settings);
                }
            };
        }

        private void AddArgs(ICollection<string> args, KeyValuePair<string, string> setting) {
            if (setting.Value != null && setting.Value != "<null>") {
                args.Add(string.Format("--{0}={1}", setting.Key, setting.Value));
            }
        }

        private async Task GenerationSucceeded(string id, SeedSettings settings) {
            var rom = Path.Join(Path.GetTempPath(), string.Format("OR_{0}.sfc", id));

            var bpsIn = Path.Join(Path.GetTempPath(), string.Format("OR_{0}.bps", id));
            var bpsOut = string.Format("{0}/patch.bps", id);
            var uploadPatch = this.AzureStorage.UploadFileAndDelete(bpsOut, bpsIn);

            var spoilerIn = Path.Join(Path.GetTempPath(), string.Format("OR_{0}_Spoiler.json", id));
            var spoilerOut = string.Format("{0}/spoiler.json", id);
            var uploadSpoiler = this.AzureStorage.UploadFileAndDelete(spoilerOut, spoilerIn);

            var metaIn = Path.Join(Path.GetTempPath(), string.Format("OR_{0}_Meta.json", id));
            var metaOut = string.Format("{0}/meta.json", id);
            var meta = this.ProcessMetadata(metaIn);
            var uploadMeta = this.AzureStorage.UploadFile(metaOut, new BinaryData(meta));

            var settingsJson = JsonSerializer.SerializeToDocument(settings, JsonOptions.Default);
            var settingsOut = string.Format("{0}/settings.json", id);
            var uploadSettings = this.AzureStorage.UploadFile(settingsOut, new BinaryData(settingsJson));

            await Task.WhenAll(uploadPatch, uploadSpoiler, uploadMeta, uploadSettings);

            this.Logger.LogDebug("Deleting file {filepath}", metaIn);
            File.Delete(metaIn);

            this.Logger.LogDebug("Deleting file {filepath}", rom);
            File.Delete(rom);

            this.Logger.LogDebug("Finished uploading seed id {id}", id);
        }

        private JsonDocument ProcessMetadata(string path) {
            JsonDocument orig;
            using (var file = File.OpenRead(path)) {
                orig = JsonDocument.Parse(file);
            }


            var processed = new Dictionary<string, JsonElement>();
            foreach (var toplevel in orig.RootElement.EnumerateObject()) {
                var value = toplevel.Value;
                if (value.ValueKind == JsonValueKind.Object && value.TryGetProperty("1", out var p1)) {
                    processed[toplevel.Name] = p1;
                } else {
                    processed[toplevel.Name] = toplevel.Value;
                }
            }

            return JsonSerializer.SerializeToDocument(processed, JsonOptions.Default);
        }

        private void GenerationFailed(string id, int exitcode) {
        }
    }
}
