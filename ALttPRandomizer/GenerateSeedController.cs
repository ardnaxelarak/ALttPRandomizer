namespace ALttPRandomizer {
    using ALttPRandomizer.Model;
    using Microsoft.AspNetCore.Mvc;

    public class GenerateController : Controller {
        [Route("/generate")]
        [HttpPost]
        public ActionResult Generate(SeedSettings settings) {
            return Content("Hello world");
        }
    }
}
