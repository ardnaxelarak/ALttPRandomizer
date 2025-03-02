using ALttPRandomizer.Model;

namespace ALttPRandomizer.Service {
    public class RandomizeService {
        public RandomizeService(IdGenerator idGenerator, Randomizer randomizer) {
            this.IdGenerator = idGenerator;
            this.Randomizer = randomizer;
        }

        private IdGenerator IdGenerator { get; }
        private Randomizer Randomizer { get; }

        public string RandomizeSeed(SeedSettings settings) {
            var id = this.IdGenerator.GenerateId();
            this.Randomizer.Randomize(id, settings);
            return id;
        }
    }
}
