namespace ALttPRandomizer.Service {
    using ALttPRandomizer.Azure;
    using System;
    using System.Collections.Generic;
    using System.Text.Json;
    using System.Threading.Tasks;

    public class SeedService {
        public SeedService(AzureStorage azureStorage) {
            this.AzureStorage = azureStorage;
        }

        private AzureStorage AzureStorage { get; }

        public async Task<IDictionary<string, object>> GetSeed(string seedId) {
            var files = await this.AzureStorage.GetFiles(seedId);

            var result = new Dictionary<string, object>();

            if (files.TryGetValue("settings.json", out var settingsData)) {
                var json = JsonDocument.Parse(settingsData.ToString());
                result["settings"] = json;
            }

            if (files.TryGetValue("meta.json", out var metaData)) {
                var json = JsonDocument.Parse(metaData.ToString());
                result["meta"] = json;
            }

            if (files.TryGetValue("spoiler.json", out var spoilerData)) {
                var json = JsonDocument.Parse(spoilerData.ToString());
                result["spoiler"] = json;
            }

            if (files.TryGetValue("patch.bps", out var patchData)) {
                result["patch.bps"] = Convert.ToBase64String(patchData.ToArray());
            }

            return result;
        }
    }
}
