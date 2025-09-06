namespace ALttPRandomizer.Azure {
    using global::Azure.Storage.Blobs;
    using global::Azure.Storage.Blobs.Models;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;

    public class AzureStorage {
        public AzureStorage(BlobContainerClient blobClient, ILogger<AzureStorage> logger) {
            this.BlobClient = blobClient;
            this.Logger = logger;
        }

        private ILogger<AzureStorage> Logger { get; }
        private BlobContainerClient BlobClient { get; }

        public async Task DeleteFile(string name) {
            await BlobClient.DeleteBlobAsync(name);
        }

        public async Task UploadFile(string name, Stream data) {
            await BlobClient.UploadBlobAsync(name, data);
        }

        public async Task UploadFile(string name, BinaryData data) {
            await BlobClient.UploadBlobAsync(name, data);
        }

        public async Task UploadFileFromSource(string name, string filepath) {
            using (var stream = new FileStream(filepath, FileMode.Open, FileAccess.Read)) {
                this.Logger.LogDebug("Uploading file {filepath} -> {name}", filepath, name);
                await this.UploadFile(name, stream);
            }
        }

        public async Task UploadFileAndDelete(string name, string filepath) {
            await this.UploadFileFromSource(name, filepath);

            this.Logger.LogDebug("Deleting file {filepath}", filepath);
            File.Delete(filepath);
        }

        public async Task<Dictionary<string, BinaryData>> GetFiles(string seedId) {
            var prefix = seedId + "/";
            var blobs = this.BlobClient.GetBlobsAsync(prefix: prefix);

            var data = new Dictionary<string, BinaryData>();

            await foreach (var blob in blobs) {
                var result = await this.BlobClient.GetBlobClient(blob.Name).DownloadContentAsync();

                if (!blob.Name.StartsWith(prefix)) {
                    this.Logger.LogWarning("Found prefix mismatch for seed id {seedId}, blob name {blobName}", seedId, blob.Name);
                    continue;
                }

                var suffix = blob.Name.Substring(prefix.Length);

                data[suffix] = result.Value.Content;
            }

            return data;
        }

        public async Task<DateTimeOffset> GetFileCreation(string filename) {
            var blob = this.BlobClient.GetBlobClient(filename);
            var blobProperties = await blob.GetPropertiesAsync();
            return blobProperties.Value.CreatedOn;
        }
    }
}
