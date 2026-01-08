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
    using System.Linq;
    using System.Text.Json;
    using System.Threading.Tasks;

    public class BaseRandomizer : IRandomizer {
        public const string Name = "base";
        public const string DungeonMapName = "dungeon_map";

        public BaseRandomizer(
                AzureStorage azureStorage,
                CommonSettingsProcessor settingsProcessor,
                IdGenerator idGenerator,
                IOptionsMonitor<ServiceOptions> optionsMonitor,
                ILogger<BaseRandomizer> logger) {
            this.AzureStorage = azureStorage;
            this.SettingsProcessor = settingsProcessor;
            this.IdGenerator = idGenerator;
            this.OptionsMonitor = optionsMonitor;
            this.Logger = logger;
        }

        private CommonSettingsProcessor SettingsProcessor { get; }
        private AzureStorage AzureStorage { get; }
        private IdGenerator IdGenerator { get; }
        private IOptionsMonitor<ServiceOptions> OptionsMonitor { get; }
        private ILogger<BaseRandomizer> Logger { get; }
        private ServiceOptions Configuration => OptionsMonitor.CurrentValue;

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

        private IList<string> GetArgs(SeedSettings settings) {
            var args = new List<string>() {
                "--reduce_flashing",
                "--quickswap",
                "--shufflelinks",
                "--shuffletavern",
            };

            if (settings.DoorShuffle == DoorShuffle.Vanilla) {
                settings.DoorTypeMode = DoorTypeMode.Original;
            }

            foreach (var arg in SettingsProcessor.GetSettings(settings.Randomizer, settings)) {
                args.Add(arg);
            }

            if (settings.DoorShuffle != DoorShuffle.Vanilla || settings.DropShuffle != DropShuffle.Vanilla
                || (settings.Pottery != Pottery.Vanilla && settings.Pottery != Pottery.Cave)) {
                args.Add("--dungeon_counters=on");
            }

            return args;
        }

        private async Task StartProcess(string randomizerName, string id, IEnumerable<string> settings, Func<int, Task> completed) {
            var start = new ProcessStartInfo() {
                FileName = Configuration.PythonPath,
                WorkingDirectory = Configuration.RandomizerPaths[randomizerName],
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

            foreach (var arg in settings) {
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
                try {
                    await completed.Invoke(process.ExitCode);
                } catch (Exception ex) {
                    this.Logger.LogError(ex, "Error while invoking completion of randomizer generation.");
                }
            };
        }

        public async Task Randomize(string id, SeedSettings settings) {
            Logger.LogDebug("Recieved request for id {id} to randomize settings {@settings}", id, settings);

            await StartProcess(this.SettingsProcessor.GetRandomizerName(settings.Randomizer), id, this.GetArgs(settings), async exitcode => {
                if (exitcode != 0) {
                    await GenerationFailed(id, exitcode);
                } else {
                    await SingleSucceeded(id);
                }
            });

            var settingsJson = JsonSerializer.SerializeToDocument(settings, JsonOptions.Default);
            var settingsOut = string.Format("{0}/settings.json", id);
            await AzureStorage.UploadFile(settingsOut, new BinaryData(settingsJson));
        }

        public async Task RandomizeMultiworld(string id, IList<SeedSettings> settings) {
            var randomizerName = this.SettingsProcessor.GetRandomizerName(settings[0].Randomizer);
            Logger.LogDebug("Recieved request for id {id} to randomize multiworld settings {@settings}", id, settings);

            var names = settings.Select(s => s.PlayerName.Replace(' ', '_')).ToList();

            var args = settings.Select((s, idx) => string.Format("--p{0}={1}", idx + 1, string.Join(" ", this.GetArgs(s))))
                .Append(string.Format("--names={0}", string.Join(",", names)))
                .Append(string.Format("--multi={0}", settings.Count));

            await StartProcess(randomizerName, id, args, async exitcode => {
                if (exitcode != 0) {
                    await GenerationFailed(id, exitcode);
                } else {
                    await MultiSucceeded(id, settings, names);
                }
            });

            var settingsJson = JsonSerializer.SerializeToDocument(settings, JsonOptions.Default);
            var settingsOut = string.Format("{0}/settings.json", id);
            await AzureStorage.UploadFile(settingsOut, new BinaryData(settingsJson));
        }

        private async Task SingleSucceeded(string id) {
            try {
                var basename = string.Format("OR_{0}", id);
                await this.UploadFiles(id, basename, 1, null);

                var metaIn = Path.Join(Path.GetTempPath(), string.Format("OR_{0}_Meta.json", id));
                Logger.LogDebug("Deleting file {filepath}", metaIn);
                File.Delete(metaIn);

                var spoilerIn = Path.Join(Path.GetTempPath(), string.Format("OR_{0}_Spoiler.json", id));
                Logger.LogDebug("Deleting file {filepath}", spoilerIn);
                File.Delete(spoilerIn);

                Logger.LogInformation("Finished uploading seed id {id}", id);
            } finally {
                var generating = string.Format("{0}/generating", id);
                await AzureStorage.DeleteFile(generating);
            }
        }

        private async Task UploadFiles(string id, string basename, int playerNum, string? parentId) {
            var tasks = new List<Task>();

            var rom = Path.Join(Path.GetTempPath(), string.Format("{0}.sfc", basename));
            Logger.LogDebug("Deleting file {filepath}", rom);
            File.Delete(rom);

            var bpsIn = Path.Join(Path.GetTempPath(), string.Format("{0}.bps", basename));
            var bpsOut = string.Format("{0}/patch.bps", id);
            tasks.Add(this.AzureStorage.UploadFileAndDelete(bpsOut, bpsIn));

            var spoilerIn = Path.Join(Path.GetTempPath(), string.Format("OR_{0}_Spoiler.json", parentId ?? id));
            var spoilerOut = string.Format("{0}/spoiler.json", id);
            tasks.Add(this.AzureStorage.UploadFileFromSource(spoilerOut, spoilerIn));

            var metaIn = Path.Join(Path.GetTempPath(), string.Format("OR_{0}_Meta.json", parentId ?? id));
            var metaOut = string.Format("{0}/meta.json", id);
            var meta = ProcessMetadata(metaIn, playerNum);
            tasks.Add(this.AzureStorage.UploadFile(metaOut, new BinaryData(meta)));

            if (parentId != null) {
                var parentOut = string.Format("{0}/parent", id);
                tasks.Add(this.AzureStorage.UploadFile(parentOut, new BinaryData(parentId)));
            }

            await Task.WhenAll(tasks);
        }

        private async Task MultiSucceeded(string id, IList<SeedSettings> settings, IList<string> names) {
            var tasks = new List<Task>();
            var subIds = new List<string>();
            var worlds = new List<object>();

            try {
                for (var i = 0; i < settings.Count; i++) {
                    var basename = string.Format("OR_{0}_P{1}_{2}", id, i + 1, names[i]);
                    var randomId = this.IdGenerator.GenerateId();
                    subIds.Add(randomId);
                    tasks.Add(this.UploadFiles(randomId, basename, i + 1, id));

                    worlds.Add(new { Name = settings[i].PlayerName, Id = randomId });

                    var settingsJson = JsonSerializer.SerializeToDocument(settings[i], JsonOptions.Default);
                    var settingsOut = string.Format("{0}/settings.json", randomId);
                    tasks.Add(this.AzureStorage.UploadFile(settingsOut, new BinaryData(settingsJson)));
                }

                var worldsJson = JsonSerializer.SerializeToDocument(worlds, JsonOptions.Default);
                var worldsOut = string.Format("{0}/worlds.json", id);

                tasks.Add(this.AzureStorage.UploadFile(worldsOut, new BinaryData(worldsJson)));

                await Task.WhenAll(tasks);

                var metaIn = Path.Join(Path.GetTempPath(), string.Format("OR_{0}_Meta.json", id));
                var metaOut = string.Format("{0}/meta.json", id);
                var uploadMeta = AzureStorage.UploadFileAndDelete(metaOut, metaIn);

                var spoilerIn = Path.Join(Path.GetTempPath(), string.Format("OR_{0}_Spoiler.json", id));
                var spoilerOut = string.Format("{0}/spoiler.json", id);
                var uploadSpoiler = AzureStorage.UploadFileAndDelete(spoilerOut, spoilerIn);

                var multidataIn = Path.Join(Path.GetTempPath(), string.Format("OR_{0}_multidata", id));
                var multidataOut = string.Format("{0}/multidata", id);
                var uploadMultidata = AzureStorage.UploadFileAndDelete(multidataOut, multidataIn);

                await Task.WhenAll(uploadMeta, uploadSpoiler, uploadMultidata);

                Logger.LogInformation("Finished uploading multiworld id {id}", id);
            } finally {
                var generating = string.Format("{0}/generating", id);
                var deleteGenerating = AzureStorage.DeleteFile(generating);
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

        private async Task GenerationFailed(string id, int exitcode) {
            var generating = string.Format("{0}/generating", id);
            var deleteGenerating = AzureStorage.DeleteFile(generating);

            await Task.WhenAll(deleteGenerating);
        }
    }
}
