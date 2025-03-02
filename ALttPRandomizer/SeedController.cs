namespace ALttPRandomizer {
    using ALttPRandomizer.Model;
    using ALttPRandomizer.Service;
    using Microsoft.AspNetCore.Mvc;
    using System.Threading.Tasks;

    public class GenerateController : Controller {
        public GenerateController(RandomizeService randomizeService, SeedService seedService) {
            this.RandomizeService = randomizeService;
            this.SeedService = seedService;
        }

        private RandomizeService RandomizeService { get; }
        private SeedService SeedService { get; }

        [Route("/generate")]
        [HttpPost]
        public ActionResult Generate(SeedSettings settings) {
            var id = this.RandomizeService.RandomizeSeed(settings);
            var url = string.Format("/seed/{0}", id);
            return Accepted(url, id);
        }

        [Route("/seed/{id}")]
        [HttpGet]
        public async Task<ActionResult> GetSeed(string id) {
            var result = await this.SeedService.GetSeed(id);
            return Ok(result);
        }
    }
}
