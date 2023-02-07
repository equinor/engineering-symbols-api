namespace EngineeringSymbols.Api.Endpoints;

public static class EndpointsInfrastructure
{
    public static WebApplication AddEndpoints(this WebApplication app)
    {
        var symbols = app.MapGroup("/symbols");
        
        symbols.MapGet("/", GetEngineeringSymbols.GetAllAsync);
        symbols.MapGet("/{id}", GetEngineeringSymbols.GetByIdAsync);
        symbols.MapPost("/", UploadEngineeringSymbol.UploadAsync);

        return app;
    }
}