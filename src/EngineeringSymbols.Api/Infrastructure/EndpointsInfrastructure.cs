using System.Security.Claims;
using EngineeringSymbols.Api.Endpoints;
using EngineeringSymbols.Api.Infrastructure.Auth;
using EngineeringSymbols.Api.Repositories.Fuseki;
using EngineeringSymbols.Tools.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using Swashbuckle.AspNetCore.Annotations;

namespace EngineeringSymbols.Api.Infrastructure;

internal delegate Task<IResult> CreateFromJsonHandler(IEngineeringSymbolService symbolService, bool? validationOnly, ClaimsPrincipal user, EngineeringSymbolCreateDto createDto);

public static class EndpointsInfrastructure
{
    public static WebApplication AddEndpoints(this WebApplication app)
    {
        var noAuth = app.MapGroup("/symbols");

        noAuth.MapGet("/", async (IEngineeringSymbolService symbolService, bool? allVersions) 
            => await symbolService
                .GetSymbolsPublicAsync(allVersions ?? false)
                .Match(TypedResults.Ok, exception =>  EndpointsCommon.OnFailure(exception, app.Logger)))
            .WithTags("Anonymous")
            //.WithName("GetAllIds")
            .WithMetadata(new SwaggerOperationAttribute("Get all Engineering Symbols", "Get all Engineering Symbols. If query parameter 'allVersions' is missing or 'false' only the latest version of a symbol is returned. If 'allVersions' is true, all versions of every symbol is returned."))
            .Produces<List<EngineeringSymbolPublicDto>>()
            .RequireRateLimiting(RateLimiterPolicy.Fixed)
            .AllowAnonymous();
        
        noAuth.MapGet("/{idOrKey}", async (IEngineeringSymbolService symbolService, string idOrKey) 
                => await symbolService
                    .GetSymbolByIdOrKeyPublicAsync(idOrKey)
                    .Match(TypedResults.Ok, exception =>  EndpointsCommon.OnFailure(exception, app.Logger)))
            .WithTags("Anonymous")
            .WithMetadata(new SwaggerOperationAttribute("Get an Engineering Symbol by Id or Key", "Get an Engineering Symbol by Id or Key"))
            .Produces<EngineeringSymbolPublicDto>()
            .RequireRateLimiting(RateLimiterPolicy.Fixed)
            .AllowAnonymous();
        
        
        var auth = app.MapGroup("/admin/symbols");
        
        auth.MapGet("/", async (IEngineeringSymbolService symbolService, bool? allVersions) 
                => await symbolService
                    .GetSymbolsAsync(allVersions ?? false)
                    .Match(TypedResults.Ok, exception =>  EndpointsCommon.OnFailure(exception, app.Logger)))
            .WithTags("Authenticated")
            //.WithName("GetAllIds")
            .WithMetadata(new SwaggerOperationAttribute("Get all Engineering Symbols", "Get all Engineering Symbols. If query parameter 'allVersions' is missing or 'false' only the latest version of a symbol is returned. If 'allVersions' is true, all versions of every symbol is returned."))
            .Produces<List<EngineeringSymbolDto>>()
            //.RequireRateLimiting(RateLimiterPolicy.Fixed)
            .RequireAuthorization(Policy.ContributorOrAdmin);
        
        auth.MapPost("/fromFile", async (IEngineeringSymbolService symbolService,
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
                    .CreateSymbolFromFileAsync(user, svgFile)
                    .Match(TypedResults.Created, exception => EndpointsCommon.OnFailure(exception, app.Logger));
            })
            .WithTags("Authenticated")
            .WithMetadata(new SwaggerOperationAttribute("Create an Engineering Symbol from SVG file (upload)", "Create Engineering Symbol from SVG file (upload). If query parameter 'validationOnly' is true, only a validation of the SVG file is performed, nothing will be stored in the database."))
            .WithDescription("Create Engineering Symbol from SVG file")
            .RequireAuthorization(Policy.ContributorOrAdmin);
        
        // JSON
        auth.MapPost("/fromJson", (CreateFromJsonHandler) (async (symbolService, [FromQuery(Name = "validationOnly")] validationOnly, user, createDto) => 
            {
                if (validationOnly is true)
                {
                    return TypedResults.BadRequest();
                }
                
                return TypedResults.Created("jadda");
            }))
            .WithTags("Authenticated")
            .WithMetadata(new SwaggerOperationAttribute("Create Engineering Symbol from JSON", "Create an Engineering Symbol from JSON. If query parameter 'validationOnly' is true, only a validation of the SVG file is performed, nothing will be stored in the database."))
            .RequireAuthorization(Policy.ContributorOrAdmin);


        
        // Only for symbols that is in draft mode
        auth.MapPut("/{id}", async (IEngineeringSymbolService symbolService, EngineeringSymbolDto symbol) => Results.Problem("Endpoint not implemented"))
            .WithTags("Authenticated")
            .WithMetadata(new SwaggerOperationAttribute("Update (replace) an Engineering Symbol by Id (When Status='Draft')", "Update (replace) an Engineering Symbol by Id. Note that this will only work if the symbol has Status='Draft'. The value of the 'Status' field is ignored and 'Id' and 'Key' must match existing entry."))
            .RequireAuthorization(Policy.ContributorOrAdmin);
            
        auth.MapDelete("/{id}", async (IEngineeringSymbolService symbolService, string id)
                => await symbolService
                    .DeleteSymbolAsync(id)
                    .Match(Succ: _ => TypedResults.NoContent(), Fail: exception =>  EndpointsCommon.OnFailure(exception, app.Logger)))
            .WithTags("Authenticated")
            .WithMetadata(new SwaggerOperationAttribute("Delete an Engineering Symbol", "Delete an Engineering Symbol"))
            .RequireAuthorization(Policy.OnlyAdmins);

        auth.MapGet("/{id}/status", async (IEngineeringSymbolService symbolService, string id) => Results.Problem("Endpoint not implemented"))
            .WithTags("Authenticated")
            .WithMetadata(new SwaggerOperationAttribute("Get the Status of an Engineering Symbol", "Get the Status of an Engineering Symbol"))
            .RequireAuthorization(Policy.ContributorOrAdmin);
        
        // Only for super admins
        // Only for existing status != "accepted"
        auth.MapPut("/{id}/status", async (IEngineeringSymbolService symbolService, string id) => Results.Problem("Endpoint not implemented"))
            .WithTags("Authenticated")
            .WithMetadata(new SwaggerOperationAttribute("Set the Status of an Engineering Symbol", "Set the Status of an Engineering Symbol"))
            .RequireAuthorization(Policy.OnlyAdmins);
            
        //symbols.MapPatch("/{id}", async (IEngineeringSymbolService symbolService, string id, EngineeringSymbolUpdateDto updateDto) 
        //        => await symbolService
        //            .UpdateSymbolAsync(id, updateDto)
        //            .Match(_ => TypedResults.NoContent(), exception =>  EndpointsCommon.OnFailure(exception, app.Logger)))
        //    .WithTags("Authenticated")
        //    .RequireAuthorization(Policy.ContributorOrAdmin);
        
        if (!app.Environment.IsDevelopment()) return app;
        
        app.MapPost("/dev-fuseki/query", DevFuseki.Query)
            .WithMetadata(new SwaggerOperationAttribute("Query local fuseki", "Query local fuseki"))
            .Accepts<string>("application/sparql-query; charset=UTF-8")
            .Produces<FusekiSelectResponse>(contentType: "application/json");
            
        app.MapPost("/dev-fuseki/update", DevFuseki.Update)
            .WithMetadata(new SwaggerOperationAttribute("Query (Update) local fuseki", "Query (Update) local fuseki"))
            .Accepts<string>("application/sparql-query; charset=UTF-8")
            .Produces<string>(contentType: "text/plain");

        return app;
    }
}