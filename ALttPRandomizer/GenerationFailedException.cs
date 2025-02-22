namespace ALttPRandomizer {
    using System;

    public class GenerationFailedException : Exception {
        public GenerationFailedException(string message) : base(message) { }
    }
}
