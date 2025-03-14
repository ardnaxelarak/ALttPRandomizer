namespace ALttPRandomizer {
    using ALttPRandomizer.Model;
    using ALttPRandomizer.Service;
    using ALttPRandomizer.Settings;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class SeedController : Controller {
        public SeedController(RandomizeService randomizeService, SeedService seedService, ILogger<SeedController> logger) {
            this.RandomizeService = randomizeService;
            this.SeedService = seedService;
            this.Logger = logger;
        }

        private RandomizeService RandomizeService { get; }
        private SeedService SeedService { get; }
        private ILogger<SeedController> Logger { get; }

        [Route("/generate")]
        [HttpPost]
        public async Task<ActionResult> Generate([FromBody] SeedSettings settings) {
            if (!ModelState.IsValid) {
                return BadRequest(ModelState);
            }
            try {
                var id = await this.RandomizeService.RandomizeSeed(settings);
                var url = string.Format("/seed/{0}", id);
                return Accepted(url, id);
            } catch (InvalidSettingsException ex) {
                return BadRequest(ex.Message);
            }
        }

        [Route("/multiworld")]
        [HttpPost]
        public async Task<ActionResult> GenerateMultiworld([FromBody] IList<SeedSettings> settings) {
            if (!ModelState.IsValid) {
                return BadRequest(ModelState);
            }
            try {
                var id = await this.RandomizeService.RandomizeMultiworld(settings);
                var url = string.Format("/multi/{0}", id);
                return Accepted(url, id);
            } catch (InvalidSettingsException ex) {
                return BadRequest(ex.Message);
            }
        }

        [Route("/seed/{id}")]
        [HttpGet]
        public async Task<ActionResult> GetSeed(string id) {
            var result = await this.SeedService.GetSeed(id);
            if (result.TryGetValue("status", out var responseCode)) {
                switch (responseCode) {
                    case 200:
                        return Ok(result);
                    case 404:
                        return NotFound(result);
                    case 409:
                        return Conflict(result);
                }
            }

            this.Logger.LogWarning("Unexpected result from SeedService: {@result}", result);
            return StatusCode(500);
        }

        [Route("/multi/{id}")]
        [HttpGet]
        public async Task<ActionResult> GetMulti(string id) {
            var result = await this.SeedService.GetMulti(id);
            if (result.TryGetValue("status", out var responseCode)) {
                switch (responseCode) {
                    case 200:
                        return Ok(result);
                    case 404:
                        return NotFound(result);
                    case 409:
                        return Conflict(result);
                }
            }

            this.Logger.LogWarning("Unexpected result from SeedService: {@result}", result);
            return StatusCode(500);
        }
    }
}
