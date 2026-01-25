namespace ALttPRandomizer {
    using System;

    public class GenerationFailedException : Exception {
        public GenerationFailedException(string message, params string[] args) : base(string.Format(message, args)) { }
    }
}
