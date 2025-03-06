namespace ALttPRandomizer.Randomizers {
    using ALttPRandomizer.Model;
    using System.Threading.Tasks;

    public interface IRandomizer {
        public void Validate(SeedSettings settings);

        public Task Randomize(string id, SeedSettings settings);
    }
}
