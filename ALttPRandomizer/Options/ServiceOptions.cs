namespace ALttPRandomizer.Options {
    using System;
    using System.Collections.Generic;

    public class ServiceOptions {
        public string Baserom { get; set; } = null!;
        public string PythonPath { get; set; } = null!;
        public string FlipsPath { get; set; } = null!;
        public IList<string> AllowedCors { get; set; } = new List<string>();
        public AzureSettings AzureSettings { get; set; } = new AzureSettings();
        public IDictionary<string, string> RandomizerPaths { get; set; } = new Dictionary<string, string>();
    }

    public class AzureSettings {
        public string? ClientId { get; set; }
        public Uri BlobstoreEndpoint { get; set; } = null!;
    }
}
