namespace ALttPRandomizer {
    using ALttPRandomizer.Azure;
    using ALttPRandomizer.Options;
    using ALttPRandomizer.Randomizers;
    using ALttPRandomizer.Service;
    using ALttPRandomizer.Settings;
    using global::Azure.Identity;
    using global::Azure.Storage.Blobs;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Serilog;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    internal class Program
    {
        static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Configuration
                .AddJsonFile("appsettings.json")
                .AddJsonFile("appsettings.Development.json", true)
                .AddEnvironmentVariables();

            builder.Services.Configure<ServiceOptions>(builder.Configuration.GetSection("ALttPRandomizer"));

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .CreateLogger();

            builder.Services.AddLogging(logger => {
                logger.ClearProviders();
                logger.AddSerilog();
            });

            var provider = builder.Services.BuildServiceProvider();
            var settings = provider.GetRequiredService<IOptionsMonitor<ServiceOptions>>().CurrentValue!;
            var logger = provider.GetRequiredService<ILogger<Program>>();

            builder.Services.AddCors(options => {
                options.AddPolicy("AllowDomains", policy => {
                    foreach (var domain in settings.AllowedCors) {
                        policy.WithOrigins(domain).AllowAnyHeader();
                    }
                });
            });

            builder.Services.AddControllers()
                .AddJsonOptions(x => {
                    x.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                    x.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
                    x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower, false));
                });
            builder.Services.AddSwaggerGen();

            var options = new DefaultAzureCredentialOptions();
            
            if (settings.AzureSettings.ClientId != null) {
                options.ManagedIdentityClientId = new(settings.AzureSettings.ClientId);
            }

            var token = new DefaultAzureCredential(options);
            var seedClient = new BlobContainerClient(settings.AzureSettings.BlobstoreEndpoint, token);

            builder.Services.AddSingleton(seedClient);
            builder.Services.AddSingleton(sp => sp);
            builder.Services.AddSingleton<AzureStorage>();
            builder.Services.AddSingleton<CommonSettingsProcessor>();

            builder.Services.AddKeyedScoped<IRandomizer, BaseRandomizer>(BaseRandomizer.Name);
            builder.Services.AddKeyedScoped<IRandomizer, Apr2025Randomizer>(Apr2025Randomizer.Name);
            builder.Services.AddScoped<BaseRandomizer>();

            builder.Services.AddScoped<RandomizeService>();
            builder.Services.AddScoped<SeedService>();
            builder.Services.AddScoped<IdGenerator>();

            var app = builder.Build();

            app.UseHttpsRedirection();
            app.UseCors("AllowDomains");
            app.MapControllers();
            app.UseSwagger();
            app.UseSwaggerUI(c => {
                c.ShowCommonExtensions();
                c.EnableValidator();
            });

            app.Run();
        }
    }
}
