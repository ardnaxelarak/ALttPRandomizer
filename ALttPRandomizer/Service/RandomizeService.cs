using ALttPRandomizer.Model;
using Microsoft.Extensions.Logging;

namespace ALttPRandomizer.Service {
    public class RandomizeService {
        public RandomizeService(IdGenerator idGenerator, Randomizer randomizer, ILogger<RandomizeService> logger) {
            this.IdGenerator = idGenerator;
            this.Randomizer = randomizer;
            this.Logger = logger;
        }

        private ILogger<RandomizeService> Logger { get; }

        private IdGenerator IdGenerator { get; }
        private Randomizer Randomizer { get; }

        public string RandomizeSeed(SeedSettings settings) {
            var id = this.IdGenerator.GenerateId();
            this.Logger.LogInformation("Generating seed {seedId} with settings {@settings}", id, settings);
            this.Randomizer.Randomize(id, settings);
            return id;
        }
    }
}
