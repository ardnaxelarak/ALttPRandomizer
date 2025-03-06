namespace ALttPRandomizer.Randomizers {
    using ALttPRandomizer;
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

    public class BaseRandomizer : IRandomizer {
        public const string Name = "base";
        public const RandomizerInstance Instance = RandomizerInstance.Base;

        public BaseRandomizer(
                AzureStorage azureStorage,
                CommonSettingsProcessor settingsProcessor,
                IOptionsMonitor<ServiceOptions> optionsMonitor,
                ILogger<BaseRandomizer> logger) {
            AzureStorage = azureStorage;
            SettingsProcessor = settingsProcessor;
            OptionsMonitor = optionsMonitor;
            Logger = logger;
        }

        private CommonSettingsProcessor SettingsProcessor { get; }
        private AzureStorage AzureStorage { get; }
        private IOptionsMonitor<ServiceOptions> OptionsMonitor { get; }
        private ILogger<BaseRandomizer> Logger { get; }
        private ServiceOptions Configuration => OptionsMonitor.CurrentValue;

        public void Validate(SeedSettings settings) {
            this.SettingsProcessor.ValidateSettings(Instance, settings);
        }

        public async Task Randomize(string id, SeedSettings settings) {
            Logger.LogDebug("Recieved request for id {id} to randomize settings {@settings}", id, settings);

            var start = new ProcessStartInfo() {
                FileName = Configuration.PythonPath,
                WorkingDirectory = Configuration.RandomizerPaths[Name],
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };

            var args = start.ArgumentList;
            args.Add("DungeonRandomizer.py");
            args.Add("--rom");
            args.Add(Configuration.Baserom);
            args.Add("--bps");

            args.Add("--outputpath");
            args.Add(Path.GetTempPath());

            args.Add("--outputname");
            args.Add(id);

            args.Add("--spoiler=json");

            args.Add("--reduce_flashing");
            args.Add("--quickswap");

            args.Add("--shufflelinks");
            args.Add("--shuffletavern");

            foreach (var arg in SettingsProcessor.GetSettings(Instance, settings)) {
                args.Add(arg);
            }

            Logger.LogInformation("Randomizing with args: {args}", string.Join(" ", args));

            var process = Process.Start(start) ?? throw new GenerationFailedException("Process failed to start.");
            process.EnableRaisingEvents = true;

            process.OutputDataReceived += (_, args) => Logger.LogInformation("Randomizer STDOUT: {output}", args.Data);
            process.ErrorDataReceived += (_, args) => Logger.LogInformation("Randomizer STDERR: {output}", args.Data);

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            process.Exited += async (sender, args) => {
                var exitcode = process.ExitCode;

                if (exitcode != 0) {
                    await GenerationFailed(id, exitcode);
                } else {
                    await GenerationSucceeded(id, settings);
                }
            };

            var settingsJson = JsonSerializer.SerializeToDocument(settings, JsonOptions.Default);
            var settingsOut = string.Format("{0}/settings.json", id);
            var uploadSettings = AzureStorage.UploadFile(settingsOut, new BinaryData(settingsJson));

            var generating = string.Format("{0}/generating", id);
            var uploadGenerating = AzureStorage.UploadFile(generating, BinaryData.Empty);

            await Task.WhenAll(uploadSettings, uploadGenerating);
        }

        private async Task GenerationSucceeded(string id, SeedSettings settings) {
            var rom = Path.Join(Path.GetTempPath(), string.Format("OR_{0}.sfc", id));

            var bpsIn = Path.Join(Path.GetTempPath(), string.Format("OR_{0}.bps", id));
            var bpsOut = string.Format("{0}/patch.bps", id);
            var uploadPatch = AzureStorage.UploadFileAndDelete(bpsOut, bpsIn);

            var spoilerIn = Path.Join(Path.GetTempPath(), string.Format("OR_{0}_Spoiler.json", id));
            var spoilerOut = string.Format("{0}/spoiler.json", id);
            var uploadSpoiler = AzureStorage.UploadFileAndDelete(spoilerOut, spoilerIn);

            var metaIn = Path.Join(Path.GetTempPath(), string.Format("OR_{0}_Meta.json", id));
            var metaOut = string.Format("{0}/meta.json", id);
            var meta = ProcessMetadata(metaIn);
            var uploadMeta = AzureStorage.UploadFile(metaOut, new BinaryData(meta));

            var generating = string.Format("{0}/generating", id);
            var deleteGenerating = AzureStorage.DeleteFile(generating);

            await Task.WhenAll(uploadPatch, uploadSpoiler, uploadMeta, deleteGenerating);

            Logger.LogDebug("Deleting file {filepath}", metaIn);
            File.Delete(metaIn);

            Logger.LogDebug("Deleting file {filepath}", rom);
            File.Delete(rom);

            Logger.LogInformation("Finished uploading seed id {id}", id);
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

        private async Task GenerationFailed(string id, int exitcode) {
            var generating = string.Format("{0}/generating", id);
            var deleteGenerating = AzureStorage.DeleteFile(generating);

            await Task.WhenAll(deleteGenerating);
        }
    }
}
