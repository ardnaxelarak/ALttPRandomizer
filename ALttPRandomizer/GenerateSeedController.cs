namespace ALttPRandomizer {
    using ALttPRandomizer.Model;
    using ALttPRandomizer.Service;
    using Microsoft.AspNetCore.Mvc;
    using System.Threading.Tasks;

    public class GenerateController : Controller {
        public GenerateController(Randomizer randomizer, SeedService seedService) {
            this.Randomizer = randomizer;
            this.SeedService = seedService;
        }

        private Randomizer Randomizer { get; }
        private SeedService SeedService { get; }

        [Route("/generate")]
        [HttpPost]
        public ActionResult Generate(SeedSettings settings) {
            var result = this.Randomizer.Randomize();
            return Ok(result);
        }

        [Route("/seed/{id}")]
        [HttpGet]
        public async Task<ActionResult> GetSeed(string id) {
            var result = await this.SeedService.GetSeed(id);
            return Ok(result);
        }
    }
}
