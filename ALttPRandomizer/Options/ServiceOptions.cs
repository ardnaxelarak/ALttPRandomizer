namespace ALttPRandomizer.Options {
    using System;
    using System.Collections.Generic;

    public class ServiceOptions {
        public string Baserom { get; set; } = null!;
        public string FlipsPath { get; set; } = null!;
        public List<string> AllowedCors { get; set; } = [];
        public AzureSettings AzureSettings { get; set; } = new();
        public Dictionary<string, GeneratorSettings> Generators { get; set; } = new();
    }

    public class GeneratorSettings {
        public string WorkingDirectory { get; set; } = null!;
        public List<string> RandomizerCommand { get; set; } = [];
    }

    public class AzureSettings {
        public string? ClientId { get; set; }
        public Uri BlobstoreEndpoint { get; set; } = null!;
    }
}
