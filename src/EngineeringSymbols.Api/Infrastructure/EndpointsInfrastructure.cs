using EngineeringSymbols.Api.Endpoints;
using EngineeringSymbols.Api.Infrastructure.Auth;
using EngineeringSymbols.Api.Repositories.Fuseki;
using Microsoft.Identity.Web;

namespace EngineeringSymbols.Api.Infrastructure;

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
            .RequireRateLimiting(RateLimiterPolicy.Fixed)
            .AllowAnonymous();
        
        symbols.MapGet("/{idOrKey}", GetEngineeringSymbols.GetByIdOrKeyAsync)
            .WithTags("Anonymous")
            .Produces<EngineeringSymbolResponseDto>()
            .RequireRateLimiting(RateLimiterPolicy.Fixed)
            .AllowAnonymous();

        symbols.MapPost("/", UploadEngineeringSymbol.UploadAsync)
            .WithTags("Authenticated")
            .RequireScope();

        symbols.MapPatch("/{id}", UpdateEngineeringSymbol.UpdateSingleAsync)
            .WithTags("Authenticated")
            .RequireAuthorization(Roles.Contributor);


        symbols.MapDelete("/{id}", DeleteEngineeringSymbols.DeleteSingleAsync)
            .WithTags("Authenticated")
            .RequireAuthorization(Roles.Admin);

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