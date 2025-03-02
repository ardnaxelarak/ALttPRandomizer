namespace ALttPRandomizer
{
    using System.Text.Json.Serialization;
    using ALttPRandomizer.Azure;
    using ALttPRandomizer.Options;
    using ALttPRandomizer.Service;
    using global::Azure.Core;
    using global::Azure.Identity;
    using global::Azure.Storage.Blobs;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

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

            builder.Services.AddLogging(lb => lb.AddConsole());

            var provider = builder.Services.BuildServiceProvider();
            var settings = provider.GetRequiredService<IOptionsMonitor<ServiceOptions>>().CurrentValue!;
            var logger = provider.GetRequiredService<ILogger<Program>>();

            builder.Services.AddCors(options => {
                options.AddPolicy("AllowDomains", policy => {
                    foreach (var domain in settings.AllowedCors) {
                        policy.WithOrigins(domain);
                    }
                });
            });

            builder.Services.AddControllers().AddJsonOptions(x =>
                x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
            builder.Services.AddSwaggerGen();

            var options = new DefaultAzureCredentialOptions();
            
            if (settings.AzureSettings.ClientId != null) {
                options.ManagedIdentityClientId = new(settings.AzureSettings.ClientId);
            }

            var token = new DefaultAzureCredential(options);
            var seedClient = new BlobContainerClient(settings.AzureSettings.BlobstoreEndpoint, token);

            builder.Services.AddSingleton(seedClient);
            builder.Services.AddSingleton<AzureStorage>();
            builder.Services.AddScoped<Randomizer>();
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
