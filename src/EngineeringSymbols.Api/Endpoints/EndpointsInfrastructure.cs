using EngineeringSymbols.Api.Repositories.Fuseki;
using Microsoft.AspNetCore.Mvc;

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

        symbols.MapPost("/", UploadEngineeringSymbol.UploadAsync);
            //.WithTags("Create");
            //.Produces<EngineeringSymbolCompleteResponseDto>(StatusCodes.Status201Created);
        
        //symbols.WithOpenApi();

        if (app.Environment.IsDevelopment())
        {
            app.MapPost("/dev-fuseki/query", DevFuseki.Query)
                .Accepts<string>("application/sparql-query; charset=UTF-8")
                .Produces<FusekiSelectResponse>(contentType: "application/json");
            
            app.MapPost("/dev-fuseki/update", DevFuseki.Update)
                .Accepts<string>("application/sparql-query; charset=UTF-8")
                .Produces<string>(contentType: "text/plain");
        }

        
        return app;
    }
}