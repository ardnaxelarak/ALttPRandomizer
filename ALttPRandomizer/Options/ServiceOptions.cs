namespace ALttPRandomizer.Options {
    using System;

    public class ServiceOptions {
        public string Baserom { get; set; } = null!;
        public string PythonPath { get; set; } = null!;
        public string RandomizerPath { get; set; } = null!;
        public AzureSettings AzureSettings { get; set; } = new AzureSettings();
    }

    public class AzureSettings {
        public string? ClientId { get; set; }
        public Uri BlobstoreEndpoint { get; set; } = null!;
    }
}
