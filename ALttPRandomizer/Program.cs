namespace ALttPRandomizer
{
    using System.Text.Json.Serialization;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;

    internal class Program
    {
        static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers().AddJsonOptions(x =>
                x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
            builder.Services.AddSwaggerGen();

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
