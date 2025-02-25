namespace ALttPRandomizer
{
    using System.Text.Json.Serialization;
    using ALttPRandomizer.Options;
    using Azure.Identity;
    using Azure.Storage.Blobs;
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
                .AddJsonFile("appsettings.Development.json")
                .AddEnvironmentVariables();

            builder.Services.Configure<ServiceOptions>(builder.Configuration.GetSection("ALttPRandomizer"));

            builder.Services.AddLogging(lb => lb.AddConsole());

            builder.Services.AddControllers().AddJsonOptions(x =>
                x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
            builder.Services.AddSwaggerGen();

            var provider = builder.Services.BuildServiceProvider();
            var settings = provider.GetRequiredService<IOptionsMonitor<ServiceOptions>>().CurrentValue!;

            var token = new DefaultAzureCredential();
            var seedClient = new BlobContainerClient(settings.AzureSettings.BlobstoreEndpoint, token);

            builder.Services.AddSingleton(seedClient);
            builder.Services.AddScoped<Randomizer, Randomizer>();
            builder.Services.AddScoped<IdGenerator, IdGenerator>();

            var app = builder.Build();

            app.UseHttpsRedirection();
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
