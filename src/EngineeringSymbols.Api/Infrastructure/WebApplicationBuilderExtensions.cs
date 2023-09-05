using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace EngineeringSymbols.Api.Infrastructure;

public static class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder AddLogging(this WebApplicationBuilder builder)
    {
        builder.Logging.ClearProviders();

        var logLevel = builder.Environment.IsProduction()
            ? LogEventLevel.Error
            : LogEventLevel.Information;

        var logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", logLevel)
            .MinimumLevel.Override("System", logLevel)
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:O} {Level:u3}] {Message:lj}{NewLine}{Exception}",
                theme: AnsiConsoleTheme.Code)
            .CreateLogger();

        builder.Logging.AddSerilog(logger);

        builder.Services.AddProblemDetails();

        return builder;
    }
}