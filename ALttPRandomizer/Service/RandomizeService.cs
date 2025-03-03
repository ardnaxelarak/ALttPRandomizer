using ALttPRandomizer.Model;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

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

        public async Task<string> RandomizeSeed(SeedSettings settings) {
            var id = this.IdGenerator.GenerateId();
            this.Logger.LogInformation("Generating seed {seedId} with settings {@settings}", id, settings);
            await this.Randomizer.Randomize(id, settings);
            return id;
        }
    }
}
