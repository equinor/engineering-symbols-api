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

        symbols.MapGet("/", async (IEngineeringSymbolService symbolService, bool? allVersions) 
            => await symbolService
                .GetSymbolsAsync(allVersions ?? false)
                .Match(TypedResults.Ok, exception =>  EndpointsCommon.OnFailure(exception, app.Logger)))
            .WithTags("Anonymous")
            .WithName("GetAllIds")
            .WithDescription("Get all Engineering Symbols (list of Id's)")
            .Produces<List<EngineeringSymbolListItemResponseDto>>()
            .RequireRateLimiting(RateLimiterPolicy.Fixed)
            .AllowAnonymous();
        
        // SVG or JSON
        symbols.MapPost("/", async (IEngineeringSymbolService symbolService,
                    [FromQuery(Name = "validationOnly")] bool? validationOnly,
                    ClaimsPrincipal user,
                    IFormFile svgFile)
                =>
            {
                if (validationOnly is true)
                {
                    return TypedResults.BadRequest();
                }
                
                return await symbolService
                    .CreateSymbolAsync(user, svgFile)
                    .Match(TypedResults.Created, exception => EndpointsCommon.OnFailure(exception, app.Logger));
            })
            .WithTags("Authenticated")
            .RequireAuthorization(Policy.ContributorOrAdmin);

        symbols.MapGet("/{idOrKey}", async (IEngineeringSymbolService symbolService, string idOrKey) 
                => await symbolService
                    .GetSymbolByIdOrKeyAsync(idOrKey)
                    .Map(symbol => symbol.ToResponseDto())
                    .Match(TypedResults.Ok, exception =>  EndpointsCommon.OnFailure(exception, app.Logger)))
            .WithTags("Anonymous")
            .Produces<EngineeringSymbolResponseDto>()
            .RequireRateLimiting(RateLimiterPolicy.Fixed)
            .AllowAnonymous();
        
        // Only for symbols that is in draft mode
        symbols.MapPut("/{id}", async () => Results.Ok())
            .WithTags("Authenticated")
            .RequireAuthorization(Policy.ContributorOrAdmin);
            
        symbols.MapDelete("/{id}", async (IEngineeringSymbolService symbolService, string id)
                => await symbolService
                    .DeleteSymbolAsync(id)
                    .Match(Succ: _ => TypedResults.NoContent(), Fail: exception =>  EndpointsCommon.OnFailure(exception, app.Logger)))
            .WithTags("Authenticated")
            .RequireAuthorization(Policy.OnlyAdmins);

        symbols.MapGet("/{id}/status", async () => Results.Ok())
            .WithTags("Authenticated")
            .RequireAuthorization(Policy.ContributorOrAdmin);
        
        // Only for super admins
        // Only for existing status != "accepted"
        symbols.MapPut("/{id}/status", async () => Results.Ok())
            .WithTags("Authenticated")
            .RequireAuthorization(Policy.OnlyAdmins);
            
        //symbols.MapPatch("/{id}", async (IEngineeringSymbolService symbolService, string id, EngineeringSymbolUpdateDto updateDto) 
        //        => await symbolService
        //            .UpdateSymbolAsync(id, updateDto)
        //            .Match(_ => TypedResults.NoContent(), exception =>  EndpointsCommon.OnFailure(exception, app.Logger)))
        //    .WithTags("Authenticated")
        //    .RequireAuthorization(Policy.ContributorOrAdmin);
        
        if (!app.Environment.IsDevelopment()) return app;
        
        app.MapPost("/dev-fuseki/query", DevFuseki.Query)
            .Accepts<string>("application/sparql-query; charset=UTF-8")
            .Produces<FusekiSelectResponse>(contentType: "application/json");
            
        app.MapPost("/dev-fuseki/update", DevFuseki.Update)
            .Accepts<string>("application/sparql-query; charset=UTF-8")
            .Produces<string>(contentType: "text/plain");

        return app;
    }
}