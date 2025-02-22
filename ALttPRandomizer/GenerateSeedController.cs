namespace ALttPRandomizer {
    using ALttPRandomizer.Model;
    using Microsoft.AspNetCore.Mvc;

    public class GenerateController : Controller {
        public GenerateController(Randomizer randomizer) {
            this.Randomizer = randomizer;
        }

        private Randomizer Randomizer { get; }

        [Route("/generate")]
        [HttpPost]
        public ActionResult Generate(SeedSettings settings) {
            var result = this.Randomizer.Randomize();
            return Content(result);
        }
    }
}
