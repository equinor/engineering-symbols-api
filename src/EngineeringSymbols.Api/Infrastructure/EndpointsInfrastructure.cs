using System.Security.Claims;
using EngineeringSymbols.Api.Endpoints;
using EngineeringSymbols.Api.Infrastructure.Auth;
using EngineeringSymbols.Api.Repositories.Fuseki;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;

namespace EngineeringSymbols.Api.Infrastructure;

public static class EndpointsInfrastructure
{
    public static WebApplication AddEndpoints(this WebApplication app)
    {
        var symbols = app.MapGroup("/symbols");

        symbols.MapGet("/", async (IEngineeringSymbolService symbolService, bool? allVersions, int? detailLevel) 
            => await symbolService
                .GetSymbolsAsync(allVersions, detailLevel)
                .Match(TypedResults.Ok, exception =>  EndpointsCommon.OnFailure(exception, app.Logger)))
            .WithTags("Anonymous")
            .WithName("GetAllIds")
            .WithDescription("Get all Engineering Symbols (list of Id's)")
            .Produces<List<EngineeringSymbolListItemResponseDto>>()
            .RequireRateLimiting(RateLimiterPolicy.Fixed)
            .AllowAnonymous();
        
        symbols.MapGet("/{idOrKey}", async (IEngineeringSymbolService symbolService, string idOrKey) 
                => await symbolService
                    .GetSymbolByIdOrKeyAsync(idOrKey)
                    .Map(symbol => symbol.ToResponseDto())
                    .Match(TypedResults.Ok, exception =>  EndpointsCommon.OnFailure(exception, app.Logger)))
            .WithTags("Anonymous")
            .Produces<EngineeringSymbolResponseDto>()
            .RequireRateLimiting(RateLimiterPolicy.Fixed)
            .AllowAnonymous();

        symbols.MapPost("/", async (IEngineeringSymbolService symbolService, ClaimsPrincipal user, IFormFile svgFile) 
                => await symbolService
                    .CreateSymbolAsync(user, svgFile)
                    .Match(TypedResults.Created, exception =>  EndpointsCommon.OnFailure(exception, app.Logger)))
            .WithTags("Authenticated")
            .RequireAuthorization(Policy.ContributorOrAdmin);

        symbols.MapPatch("/{id}", async (IEngineeringSymbolService symbolService, string id, EngineeringSymbolUpdateDto updateDto) 
                => await symbolService
                    .UpdateSymbolAsync(id, updateDto)
                    .Match(_ => TypedResults.NoContent(), exception =>  EndpointsCommon.OnFailure(exception, app.Logger)))
            .WithTags("Authenticated")
            .RequireAuthorization(Policy.ContributorOrAdmin);
        
        symbols.MapDelete("/{id}", async (IEngineeringSymbolService symbolService, string id)
                => await symbolService
                    .DeleteSymbolAsync(id)
                    .Match(Succ: _ => TypedResults.NoContent(), Fail: exception =>  EndpointsCommon.OnFailure(exception, app.Logger)))
            .WithTags("Authenticated")
            .RequireAuthorization(Policy.OnlyAdmins);

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