namespace ALttPRandomizer.Service {
    using ALttPRandomizer.Azure;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class SeedService {
        public SeedService(AzureStorage azureStorage) {
            this.AzureStorage = azureStorage;
        }

        private AzureStorage AzureStorage { get; }

        public async Task<IDictionary<string, string>> GetSeed(string seedId) {
            var files = await this.AzureStorage.GetFiles(seedId);

            var result = new Dictionary<string, string>();
            foreach (var file in files) {
                result[file.Key] = Convert.ToBase64String(file.Value.ToMemory().ToArray());
            }

            return result;
        }
    }
}
