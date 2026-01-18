namespace ALttPRandomizer {
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using ALttPRandomizer.Azure;
    using Microsoft.Extensions.Logging;

    public class ShutdownHandler {
        public ShutdownHandler(AzureStorage azureStorage, ILogger<ShutdownHandler> logger) {
            this.AzureStorage = azureStorage;
            this.Logger = logger;
        }

        private AzureStorage AzureStorage { get; }
        private ILogger<ShutdownHandler> Logger { get; }

        private HashSet<string> IdsInProgress { get; } = new();

        public void AddId(string id) {
            this.IdsInProgress.Add(id);
        }

        public void RemoveId(string id) {
            this.IdsInProgress.Remove(id);
        }

        public async Task HandleShutdown() {
            this.Logger.LogWarning("Shutdown Received.");

            List<Task> tasks = [];

            foreach (var id in this.IdsInProgress) {
                tasks.Add(AzureStorage.DeleteFile($"{id}/generating"));
            }

            this.Logger.LogInformation("Ending {count} in-progress generations.", tasks.Count);

            await Task.WhenAll(tasks);
        }
    }
}
