namespace ALttPRandomizer
{
    using System.Text.Json.Serialization;
    using ALttPRandomizer.Options;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

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

            builder.Services.AddControllers().AddJsonOptions(x =>
                x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
            builder.Services.AddSwaggerGen();

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
