using EngineeringSymbols.Api.Repositories.Fuseki;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;

namespace EngineeringSymbols.Api.Endpoints;

public static class EndpointsInfrastructure
{
    public static WebApplication AddEndpoints(this WebApplication app)
    {
        var symbols = app.MapGroup("/symbols");

        symbols.MapGet("/", GetEngineeringSymbols.GetAllAsync)
            .WithTags("Anonymous")
            .WithName("GetAllIds")
            .WithDescription("Get all Engineering Symbols (list of Id's)")
            .Produces<List<EngineeringSymbolListItemResponseDto>>()
            .AllowAnonymous();
        
        symbols.MapGet("/{idOrKey}", GetEngineeringSymbols.GetByIdOrKeyAsync)
            .WithTags("Anonymous")
            .Produces<EngineeringSymbolResponseDto>()
            .AllowAnonymous();

        symbols.MapPost("/", UploadEngineeringSymbol.UploadAsync)
            .WithTags("Authenticated")
            .RequireScope();

        symbols.MapPatch("/{id}", UpdateEngineeringSymbol.UpdateSingleAsync)
            .WithTags("Authenticated")
            .RequireAuthorization();
            
        
        symbols.MapDelete("/{id}", DeleteEngineeringSymbols.DeleteSingleAsync)
            .WithTags("Authenticated")
            .RequireAuthorization();

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