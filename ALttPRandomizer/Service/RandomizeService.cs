namespace ALttPRandomizer.Service {
    using ALttPRandomizer.Azure;
    using ALttPRandomizer.Model;
    using ALttPRandomizer.Randomizers;
    using ALttPRandomizer.Settings;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Text.Json;
    using System.Threading.Tasks;

    public class RandomizeService {
        public RandomizeService(
                IdGenerator idGenerator,
                IServiceProvider serviceProvider,
                BaseRandomizer baseRandomizer,
                AzureStorage azureStorage,
                ILogger<RandomizeService> logger) {
            this.IdGenerator = idGenerator;
            this.ServiceProvider = serviceProvider;
            this.BaseRandomizer = baseRandomizer;
            this.AzureStorage = azureStorage;
            this.Logger = logger;
        }

        private ILogger<RandomizeService> Logger { get; }

        private IdGenerator IdGenerator { get; }
        private BaseRandomizer BaseRandomizer { get; }
        private IServiceProvider ServiceProvider { get; }
        private AzureStorage AzureStorage { get; }

        public async Task<string> RandomizeSeed(SeedSettings settings, string? seedId = null) {
            var id = seedId ?? this.IdGenerator.GenerateId();
            this.Logger.LogInformation("Generating seed {seedId} with settings {@settings}", id, settings);

            var fi = typeof(RandomizerInstance).GetField(settings.Randomizer.ToString(), BindingFlags.Static | BindingFlags.Public);

            var randomizerKey = fi?.GetCustomAttribute<RandomizerNameAttribute>()?.Name;

            if (randomizerKey == null) {
                throw new InvalidSettingsException("Invalid randomizer: {0}", settings.Randomizer);
            }

            var randomizer = this.ServiceProvider.GetRequiredKeyedService<IRandomizer>(randomizerKey);
            randomizer.Validate(settings);

            await randomizer.Randomize(id, settings);
            return id;
        }

        public async Task<string> RandomizeMultiworld(IList<SeedSettings> settings, string? multiId = null) {
            var id = multiId ?? this.IdGenerator.GenerateId();
            this.Logger.LogInformation("Generating multiworld {seedId} with settings {@settings}", id, settings);

            this.BaseRandomizer.ValidateAll(settings);

            await this.BaseRandomizer.RandomizeMultiworld(id, settings);
            return id;
        }

        public async Task<IDictionary<string, object>> RetrySeed(string seedId) {
            var files = await this.AzureStorage.GetFiles(seedId);

            this.Logger.LogDebug("Found files: {@files}", files.Keys);

            var result = new Dictionary<string, object>();

            if (!files.TryGetValue("settings.json", out var settingsData)) {
                result["status"] = 404;
                result["error"] = "seed not found";
                return result;
            }

            var settingsJson = JsonDocument.Parse(settingsData.ToString());

            var settings = settingsJson.Deserialize<SeedSettings>(JsonOptions.Default) ?? new SeedSettings();

            if (files.TryGetValue("patch.bps", out var patchData)) {
                result["status"] = 409;
                result["error"] = "generation already successful";
                return result;
            }

            if (files.ContainsKey("generating")) {
                result["status"] = 409;
                result["error"] = "generation still in progress";
                return result;
            }

            await this.RandomizeSeed(settings, seedId);

            result["status"] = 202;
            return result;
        }
    }
}
