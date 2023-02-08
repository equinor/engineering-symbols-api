namespace EngineeringSymbols.Api.Endpoints;

public static class EndpointsInfrastructure
{
    public static WebApplication AddEndpoints(this WebApplication app)
    {
        var symbols = app.MapGroup("/symbols");

        symbols.MapGet("/", GetEngineeringSymbols.GetAllAsync)
            .WithTags("GetEngineeringSymbols")
            .WithName("GetAllIds")
            .WithDescription("Get all Engineering Symbols (list of Id's)")
            .Produces<List<string>>();
        
        symbols.MapGet("/{id}", GetEngineeringSymbols.GetByIdAsync)
            .WithTags("GetEngineeringSymbols")
            .Produces<EngineeringSymbolResponseDto>();

        symbols.MapPost("/", UploadEngineeringSymbol.UploadAsync)
            .WithTags("Create")
            .Produces<EngineeringSymbolCompleteResponseDto>(StatusCodes.Status201Created);
        
        symbols.WithOpenApi();
        
        return app;
    }
}