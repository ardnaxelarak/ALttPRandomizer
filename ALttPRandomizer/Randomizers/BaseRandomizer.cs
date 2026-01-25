namespace ALttPRandomizer.Randomizers {
    using ALttPRandomizer;
    using ALttPRandomizer.Azure;
    using ALttPRandomizer.Model;
    using ALttPRandomizer.Options;
    using ALttPRandomizer.Service;
    using ALttPRandomizer.Settings;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text.Json;
    using System.Threading.Tasks;

    public class BaseRandomizer : IRandomizer {
        public const int MULTI_TRIES = 100;
        public const int SINGLE_TRIES = 5;

        public BaseRandomizer(
                AzureStorage azureStorage,
                CommonSettingsProcessor settingsProcessor,
                ProcessService processService,
                IdGenerator idGenerator,
                ShutdownHandler shutdownHandler,
                IOptionsMonitor<ServiceOptions> optionsMonitor,
                ILogger<BaseRandomizer> logger) {
            this.AzureStorage = azureStorage;
            this.SettingsProcessor = settingsProcessor;
            this.ProcessService = processService;
            this.IdGenerator = idGenerator;
            this.ShutdownHandler = shutdownHandler;
            this.OptionsMonitor = optionsMonitor;
            this.Logger = logger;
        }

        private CommonSettingsProcessor SettingsProcessor { get; }
        private AzureStorage AzureStorage { get; }
        private ProcessService ProcessService { get; }
        private IdGenerator IdGenerator { get; }
        private IOptionsMonitor<ServiceOptions> OptionsMonitor { get; }
        private ILogger<BaseRandomizer> Logger { get; }
        private ServiceOptions Configuration => OptionsMonitor.CurrentValue;
        private ShutdownHandler ShutdownHandler { get; }

        public void Validate(SeedSettings settings) {
            this.SettingsProcessor.ValidateSettings(settings.Randomizer, settings);
        }

        public void ValidateAll(IList<SeedSettings> settings) {
            foreach (var settingsItem in settings) {
                this.Validate(settingsItem);
                if (string.IsNullOrWhiteSpace(settingsItem.PlayerName)) {
                    throw new InvalidSettingsException("PlayerNames must be non-empty");
                }
            }
        }

        private List<string> GetArgs(SeedSettings settings) {
            var args = new List<string>();

            if (settings.Randomizer != RandomizerInstance.Apr2025) {
                args.Add("--shuffletavern");
            }

            if (settings.DoorShuffle == DoorShuffle.Vanilla) {
                settings.DoorTypeMode = DoorTypeMode.Original;
            }

            foreach (var arg in SettingsProcessor.GetSettings(settings.Randomizer, settings)) {
                args.Add(arg);
            }

            if (settings.DoorShuffle != DoorShuffle.Vanilla || settings.DropShuffle != DropShuffle.Vanilla
                || (settings.PotShuffle != PotShuffle.Vanilla && settings.PotShuffle != PotShuffle.Cave)) {
                args.Add("--dungeon_counters=on");
            }

            return args;
        }

        private async Task StartProcess(GeneratorSettingsAttribute generatorSettings, string id, IEnumerable<string> settings, Func<int, Task> completed) {
            var generatorConfigSettings = this.Configuration.Generators[generatorSettings.Name];

            string[] args = [
                .. generatorConfigSettings.RandomizerCommand,
                .. generatorSettings.Args,
                "--rom",
                Configuration.Baserom,
                "--outputpath",
                Path.GetTempPath(),
                "--outputname",
                id,
                .. settings,
            ];

            var generating = string.Format("{0}/generating", id);
            await AzureStorage.UploadFile(generating, BinaryData.Empty);

            var process = this.ProcessService.StartProcess($"Generation {id}", generatorConfigSettings.WorkingDirectory, args);

            this.ShutdownHandler.AddId(id);

            process.Exited += async (sender, args) => {
                try {
                    this.ShutdownHandler.RemoveId(id);
                    Logger.LogInformation("Generation {id} has exited with exit code {exitCode}", id, process.ExitCode);
                    await completed.Invoke(process.ExitCode);
                } catch (Exception ex) {
                    this.Logger.LogError(ex, "Error while invoking completion of randomizer generation {id}.", id);
                }
            };
        }

        public async Task Randomize(string id, SeedSettings settings, bool uploadSettings = true) {
            Logger.LogDebug("Recieved request for id {id} to randomize settings {@settings}", id, settings);

            var args = this.GetArgs(settings);

            if (settings.Randomizer != RandomizerInstance.Apr2025) {
                args.Append(string.Format("--tries={0}", SINGLE_TRIES));
            }

            var generatorSettings = this.SettingsProcessor.GetGeneratorSettings(settings.Randomizer);

            await StartProcess(generatorSettings, id, args, async exitcode => {
                if (exitcode != 0) {
                    await GenerationFailed(id, exitcode);
                } else {
                    await SingleSucceeded(generatorSettings, id);
                }
            });

            if (uploadSettings) {
                var settingsJson = JsonSerializer.SerializeToDocument(settings, JsonOptions.Default);
                var settingsOut = string.Format("{0}/settings.json", id);
                await AzureStorage.UploadFile(settingsOut, new BinaryData(settingsJson));
            }
        }

        public async Task RandomizeMultiworld(string id, IList<SeedSettings> settings, bool uploadSettings = true) {
            var generatorSettings = this.SettingsProcessor.GetGeneratorSettings(settings[0].Randomizer);

            Logger.LogDebug("Recieved request for id {id} to randomize multiworld settings {@settings}", id, settings);

            var names = settings.Select(s => s.PlayerName.Replace(' ', '_')).ToList();

            var args = settings.Select((s, idx) => string.Format("--p{0}={1}", idx + 1, string.Join(" ", this.GetArgs(s))))
                .Append(string.Format("--names={0}", string.Join(",", names)))
                .Append(string.Format("--multi={0}", settings.Count))
                .Append(string.Format("--tries={0}", MULTI_TRIES));

            await StartProcess(generatorSettings, id, args, async exitcode => {
                if (exitcode != 0) {
                    await GenerationFailed(id, exitcode);
                } else {
                    await MultiSucceeded(generatorSettings, id, settings, names);
                }
            });

            if (uploadSettings) {
                var settingsJson = JsonSerializer.SerializeToDocument(settings, JsonOptions.Default);
                var settingsOut = string.Format("{0}/settings.json", id);
                await AzureStorage.UploadFile(settingsOut, new BinaryData(settingsJson));
            }
        }

        private async Task<int> GeneratePatch(string id, string basename) {
            var tempPath = Path.GetTempPath();
            var bps = Path.Join(tempPath, string.Format("{0}.bps", basename));
            var sfc = Path.Join(tempPath, string.Format("{0}.sfc", basename));

            var process = this.ProcessService.StartProcess($"Generation {id}", tempPath, this.Configuration.FlipsPath, "--create", this.Configuration.Baserom, sfc, bps);

            await process.WaitForExitAsync();

            return process.ExitCode;
        }

        private async Task SingleSucceeded(GeneratorSettingsAttribute generatorSettings, string id) {
            try {
                var basename = $"{generatorSettings.Prefix}{id}";
                if (generatorSettings.RequireFlips) {
                    var exitCode = await GeneratePatch(id, basename);

                    if (exitCode != 0) {
                        await this.GenerationFailed(id, exitCode);
                        return;
                    }
                }

                await this.UploadFiles(id, basename);

                var metaIn = Path.Join(Path.GetTempPath(), string.Format("{0}_Meta.json", basename));
                Logger.LogDebug("Deleting file {filepath}", metaIn);
                File.Delete(metaIn);

                var spoilerIn = Path.Join(Path.GetTempPath(), string.Format("{0}_Spoiler.json", basename));
                Logger.LogDebug("Deleting file {filepath}", spoilerIn);
                File.Delete(spoilerIn);

                Logger.LogInformation("Finished uploading seed id {id}", id);
            } finally {
                await AzureStorage.DeleteFile($"{id}/generating");
            }
        }

        private async Task UploadFiles(string id, string basename, int playerNum = 1, string playerSuffix = "") {
            var tasks = new List<Task>();

            var rom = Path.Join(Path.GetTempPath(), string.Format("{0}{1}.sfc", basename, playerSuffix));
            Logger.LogDebug("Deleting file {filepath}", rom);
            File.Delete(rom);

            var bpsIn = Path.Join(Path.GetTempPath(), string.Format("{0}{1}.bps", basename, playerSuffix));
            tasks.Add(this.AzureStorage.UploadFileAndDelete($"{id}/patch.bps", bpsIn));

            var spoilerIn = Path.Join(Path.GetTempPath(), string.Format("{0}_Spoiler.json", basename));
            tasks.Add(this.AzureStorage.UploadFileFromSource($"{id}/spoiler.json", spoilerIn));

            var metaIn = Path.Join(Path.GetTempPath(), string.Format("{0}_Meta.json", basename));
            var meta = ProcessMetadata(metaIn, playerNum);
            tasks.Add(this.AzureStorage.UploadFile($"{id}/meta.json", new BinaryData(meta)));

            await Task.WhenAll(tasks);
        }

        private async Task MultiSucceeded(GeneratorSettingsAttribute generatorSettings, string id, IList<SeedSettings> settings, List<string> names) {
            var tasks = new List<Task>();
            var subIds = new List<string>();
            var worlds = new List<object>();

            try {
                var basename = $"{generatorSettings.Prefix}{id}";

                for (var i = 0; i < settings.Count; i++) {
                    var playerSuffix = $"_P{i + 1}_{names[i]}";
                    var randomId = this.IdGenerator.GenerateId();
                    subIds.Add(randomId);

                    Task<int> flipsTask;
                    if (generatorSettings.RequireFlips) {
                        flipsTask = this.GeneratePatch(id, $"{basename}{playerSuffix}");
                    } else {
                        flipsTask = Task.FromResult(0);
                    }

                    tasks.Add(flipsTask.ContinueWith(exitCode => {
                        if (exitCode.Result != 0) {
                            this.Logger.LogWarning("Generation {id} - flips failed with exit code {exitCode}", id, exitCode);
                        }

                        return this.UploadFiles(randomId, basename, i + 1, playerSuffix);
                    }));

                    tasks.Add(this.AzureStorage.UploadFile($"{randomId}/parent", new BinaryData(id)));

                    worlds.Add(new { Name = settings[i].PlayerName, Id = randomId });

                    var settingsJson = JsonSerializer.SerializeToDocument(settings[i], JsonOptions.Default);
                    tasks.Add(this.AzureStorage.UploadFile($"{randomId}/settings.json", new BinaryData(settingsJson)));
                }

                var worldsJson = JsonSerializer.SerializeToDocument(worlds, JsonOptions.Default);
                tasks.Add(this.AzureStorage.UploadFile($"{id}/worlds.json", new BinaryData(worldsJson)));

                await Task.WhenAll(tasks);

                var metaIn = Path.Join(Path.GetTempPath(), string.Format("{0}_Meta.json", basename));
                var uploadMeta = AzureStorage.UploadFileAndDelete($"{id}/meta.json", metaIn);

                var spoilerIn = Path.Join(Path.GetTempPath(), string.Format("{0}_Spoiler.json", basename));
                var uploadSpoiler = AzureStorage.UploadFileAndDelete($"{id}/spoiler.json", spoilerIn);

                var multidataIn = Path.Join(Path.GetTempPath(), string.Format("{0}_multidata", basename));
                var uploadMultidata = AzureStorage.UploadFileAndDelete($"{id}/multidata", multidataIn);

                await Task.WhenAll(uploadMeta, uploadSpoiler, uploadMultidata);

                Logger.LogInformation("Finished uploading multiworld id {id}", id);
            } finally {
                var generating = string.Format("{0}/generating", id);
                await AzureStorage.DeleteFile(generating);
            }
        }

        private JsonDocument ProcessMetadata(string path, int playerNum) {
            JsonDocument orig;
            using (var file = File.OpenRead(path)) {
                orig = JsonDocument.Parse(file);
            }

            var processed = new Dictionary<string, JsonElement>();
            foreach (var toplevel in orig.RootElement.EnumerateObject()) {
                var value = toplevel.Value;
                if (value.ValueKind == JsonValueKind.Object && value.TryGetProperty(playerNum.ToString(), out var p)) {
                    processed[toplevel.Name] = p;
                } else {
                    processed[toplevel.Name] = toplevel.Value;
                }
            }

            return JsonSerializer.SerializeToDocument(processed, JsonOptions.Default);
        }

        private async Task GenerationFailed(string id, int _) {
            var generating = string.Format("{0}/generating", id);
            await AzureStorage.DeleteFile(generating);
        }
    }
}
