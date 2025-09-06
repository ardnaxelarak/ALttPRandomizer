namespace ALttPRandomizer.Service {
    using global::Azure;
    using ALttPRandomizer.Azure;
    using ALttPRandomizer.Model;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json;
    using System.Threading.Tasks;

    public class SeedService {
        public SeedService(AzureStorage azureStorage, ILogger<SeedService> logger) {
            this.AzureStorage = azureStorage;
            this.Logger = logger;
        }

        private AzureStorage AzureStorage { get; }
        private ILogger<SeedService> Logger { get; }

        public async Task<IDictionary<string, object>> GetSeed(string seedId) {
            var files = await this.AzureStorage.GetFiles(seedId);

            this.Logger.LogDebug("Found files: {@files}", files.Keys);

            var result = new Dictionary<string, object>();

            if (!files.TryGetValue("settings.json", out var settingsData)) {
                result["status"] = 404;
                result["error"] = "seed not found";
                return result;
            }

            var settingsJson = JsonDocument.Parse(settingsData.ToString());
            result["settings"] = settingsJson;

            var settings = settingsJson.Deserialize<SeedSettings>(JsonOptions.Default) ?? new SeedSettings();

            if (!files.TryGetValue("patch.bps", out var patchData)) {
                if (files.ContainsKey("generating")) {
                    result["status"] = 409;
                    result["error"] = "generation still in progress";
                    return result;
                } else {
                    result["status"] = 404;
                    result["error"] = "generation failed";
                    result["retry"] = true;
                    return result;
                }
            }
            result["patch"] = Convert.ToBase64String(patchData.ToArray());

            try {
                var creationTime = await this.AzureStorage.GetFileCreation($"{seedId}/patch.bps");
                result["created"] = creationTime.ToUnixTimeSeconds();
            } catch (RequestFailedException e) {
                this.Logger.LogError(e, "Failed to get creation timestamp for seed {seedId}", seedId);
            }

            if (files.TryGetValue("meta.json", out var metaData)) {
                var json = JsonDocument.Parse(metaData.ToString());
                result["meta"] = json;
            }

            if (files.TryGetValue("parent", out var parent)) {
                result["parent"] = parent.ToString();
            }

            if (settings.Race != RaceMode.Race && files.TryGetValue("spoiler.json", out var spoilerData)) {
                var json = JsonDocument.Parse(spoilerData.ToString());
                result["spoiler"] = json;
            }

            result["status"] = 200;

            return result;
        }

        public async Task<IDictionary<string, object>> GetMulti(string multiId) {
            var files = await this.AzureStorage.GetFiles(multiId);

            this.Logger.LogDebug("Found files: {@files}", files.Keys);

            var result = new Dictionary<string, object>();

            if (!files.TryGetValue("settings.json", out var settingsData)) {
                result["status"] = 404;
                result["error"] = "multi not found";
                return result;
            }

            var settingsJson = JsonDocument.Parse(settingsData.ToString());
            result["settings"] = settingsJson;

            var settings = settingsJson.Deserialize<IList<SeedSettings>>(JsonOptions.Default) ?? new List<SeedSettings>();

            if (!files.TryGetValue("multidata", out var multidata)) {
                if (files.ContainsKey("generating")) {
                    result["status"] = 409;
                    result["error"] = "generation still in progress";
                    return result;
                } else {
                    result["status"] = 404;
                    result["error"] = "generation failed";
                    return result;
                }
            }
            result["multidata"] = Convert.ToBase64String(multidata.ToArray());

            if (files.TryGetValue("meta.json", out var metaData)) {
                var json = JsonDocument.Parse(metaData.ToString());
                result["meta"] = json;
            }

            if (files.TryGetValue("worlds.json", out var worlds)) {
                var json = JsonDocument.Parse(worlds.ToString());
                result["worlds"] = json;
            }

            if (!settings.Any(s => s.Race == RaceMode.Race) && files.TryGetValue("spoiler.json", out var spoilerData)) {
                var json = JsonDocument.Parse(spoilerData.ToString());
                result["spoiler"] = json;
            }

            result["status"] = 200;

            return result;
        }
    }
}
