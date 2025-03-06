namespace ALttPRandomizer.Randomizers {
    using ALttPRandomizer;
    using ALttPRandomizer.Azure;
    using ALttPRandomizer.Model;
    using ALttPRandomizer.Options;
    using ALttPRandomizer.Settings;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Text.Json;
    using System.Threading.Tasks;

    public class Apr2025Randomizer : IRandomizer {
        public const string Name = "apr2025";
        public const RandomizerInstance Instance = RandomizerInstance.Apr2025;

        public Apr2025Randomizer(
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
            args.Add("EntranceRandomizer.py");
            args.Add("--rom");
            args.Add(Configuration.Baserom);

            args.Add("--outputpath");
            args.Add(Path.GetTempPath());

            args.Add("--outputname");
            args.Add(id);

            args.Add("--json_spoiler");

            args.Add("--quickswap");

            foreach (var arg in SettingsProcessor.GetSettings(Instance, settings)) {
                args.Add(arg);
            }

            Logger.LogInformation("Randomizing with args: {args}", string.Join(" ", args));

            var generating = string.Format("{0}/generating", id);
            await AzureStorage.UploadFile(generating, BinaryData.Empty);

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
            await AzureStorage.UploadFile(settingsOut, new BinaryData(settingsJson));
        }

        private async Task GenerationSucceeded(string id, SeedSettings settings) {
            var rom = Path.Join(Path.GetTempPath(), string.Format("ER_{0}.sfc", id));

            var spoilerIn = Path.Join(Path.GetTempPath(), string.Format("ER_{0}_Spoiler.json", id));
            var spoilerOut = string.Format("{0}/spoiler.json", id);
            var uploadSpoiler = AzureStorage.UploadFileAndDelete(spoilerOut, spoilerIn);

            var metaIn = Path.Join(Path.GetTempPath(), string.Format("ER_{0}_Meta.json", id));
            var metaOut = string.Format("{0}/meta.json", id);
            var uploadMeta = AzureStorage.UploadFileAndDelete(metaOut, metaIn);

            var bpsIn = Path.Join(Path.GetTempPath(), string.Format("ER_{0}.bps", id));

            var flips = new ProcessStartInfo() {
                FileName = Configuration.FlipsPath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };
            var args = flips.ArgumentList;
            args.Add("--create");
            args.Add(Configuration.Baserom);
            args.Add(rom);
            args.Add(bpsIn);

            var process = Process.Start(flips) ?? throw new GenerationFailedException("Process failed to start.");
            process.EnableRaisingEvents = true;

            process.OutputDataReceived += (_, args) => Logger.LogInformation("flips STDOUT: {output}", args.Data);
            process.ErrorDataReceived += (_, args) => Logger.LogInformation("flips STDERR: {output}", args.Data);

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await process.WaitForExitAsync();

            if (process.ExitCode != 0) {
                await this.GenerationFailed(id, process.ExitCode);
                return;
            }

            var bpsOut = string.Format("{0}/patch.bps", id);
            var uploadPatch = AzureStorage.UploadFileAndDelete(bpsOut, bpsIn);

            var generating = string.Format("{0}/generating", id);
            var deleteGenerating = AzureStorage.DeleteFile(generating);

            await Task.WhenAll(uploadPatch, uploadSpoiler, uploadMeta, deleteGenerating);

            Logger.LogDebug("Deleting file {filepath}", rom);
            File.Delete(rom);

            Logger.LogInformation("Finished uploading seed id {id}", id);
        }

        private async Task GenerationFailed(string id, int exitcode) {
            var generating = string.Format("{0}/generating", id);
            var deleteGenerating = AzureStorage.DeleteFile(generating);

            await Task.WhenAll(deleteGenerating);
        }
    }
}
