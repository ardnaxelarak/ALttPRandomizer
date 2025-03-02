namespace ALttPRandomizer {
    using System.Text.Json;
    using System.Text.Json.Serialization;

    public static class JsonOptions {
        public static JsonSerializerOptions Default = new JsonSerializerOptions() {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            NumberHandling = JsonNumberHandling.Strict,
        }.WithStringEnum();

        public static JsonSerializerOptions WithStringEnum(this JsonSerializerOptions options) {
            options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower, false));
            return options;
        }
    }
}
