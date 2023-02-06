namespace EngineeringSymbols.Api.Endpoints;

public static class EndpointsInfrastructure
{
    public static WebApplication AddEndpoints(this WebApplication app)
    {
        var symbols = app.MapGroup("/symbols");
        
        symbols.MapPost("/", UploadEngineeringSymbol.Endpoint);

        return app;
    }
}