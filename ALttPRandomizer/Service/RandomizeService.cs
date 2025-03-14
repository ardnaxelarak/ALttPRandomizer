namespace ALttPRandomizer.Service {
    using ALttPRandomizer.Model;
    using ALttPRandomizer.Randomizers;
    using ALttPRandomizer.Settings;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Threading.Tasks;

    public class RandomizeService {
        public RandomizeService(IdGenerator idGenerator, IServiceProvider serviceProvider, BaseRandomizer baseRandomizer, ILogger<RandomizeService> logger) {
            this.IdGenerator = idGenerator;
            this.ServiceProvider = serviceProvider;
            this.BaseRandomizer = baseRandomizer;
            this.Logger = logger;
        }

        private ILogger<RandomizeService> Logger { get; }

        private IdGenerator IdGenerator { get; }
        private BaseRandomizer BaseRandomizer { get; }
        private IServiceProvider ServiceProvider { get; }

        public async Task<string> RandomizeSeed(SeedSettings settings) {
            var id = this.IdGenerator.GenerateId();
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

        public async Task<string> RandomizeMultiworld(IList<SeedSettings> settings) {
            var id = this.IdGenerator.GenerateId();
            this.Logger.LogInformation("Generating multiworld {seedId} with settings {@settings}", id, settings);

            this.BaseRandomizer.ValidateAll(settings);

            await this.BaseRandomizer.RandomizeMultiworld(id, settings);
            return id;
        }
    }
}
