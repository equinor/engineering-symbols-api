using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace EngineeringSymbols.Api.Infrastructure;

public static class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder AddLogging(this WebApplicationBuilder builder)
    {
        builder.Logging.ClearProviders();

        var configurationBuilder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile(path: "appsettings.json", optional: false,
                reloadOnChange: builder.Environment.IsDevelopment());

#if DEBUG
        configurationBuilder.AddJsonFile($"appsettings.Development.json", true, reloadOnChange: true);
#endif
        
        var logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configurationBuilder.Build())
            .CreateLogger();

        builder.Logging.AddSerilog(logger);



        return builder;
    }
}