namespace ALttPRandomizer {
    using ALttPRandomizer.Azure;
    using ALttPRandomizer.Model;
    using ALttPRandomizer.Options;
    using ALttPRandomizer.Settings;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
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
                    await this.GenerationSucceeded(id);
                }
            };
        }

        private void AddArgs(ICollection<string> args, KeyValuePair<string, string> setting) {
            if (setting.Value != null && setting.Value != "<null>") {
                args.Add(string.Format("--{0}={1}", setting.Key, setting.Value));
            }
        }

        private async Task GenerationSucceeded(string id) {
            var rom = Path.Join(Path.GetTempPath(), string.Format("OR_{0}.sfc", id));

            var bpsIn = Path.Join(Path.GetTempPath(), string.Format("OR_{0}.bps", id));
            var bpsOut = string.Format("{0}/patch.bps", id);

            var spoilerIn = Path.Join(Path.GetTempPath(), string.Format("OR_{0}_Spoiler.txt", id));
            var spoilerOut = string.Format("{0}/spoiler.txt", id);

            var uploadPatch = this.AzureStorage.UploadFileAndDelete(bpsOut, bpsIn);
            var uploadSpoiler = this.AzureStorage.UploadFileAndDelete(spoilerOut, spoilerIn);

            await Task.WhenAll(uploadPatch, uploadSpoiler);

            this.Logger.LogDebug("Deleting file {filepath}", rom);
            File.Delete(rom);

            this.Logger.LogDebug("Finished uploading seed id {id}", id);
        }

        private void GenerationFailed(string id, int exitcode) {
        }
    }
}
