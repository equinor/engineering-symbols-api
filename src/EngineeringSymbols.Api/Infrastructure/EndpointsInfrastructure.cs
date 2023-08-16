using System.Security.Claims;
using EngineeringSymbols.Api.Endpoints;
using EngineeringSymbols.Api.Infrastructure.Auth;
using EngineeringSymbols.Api.Repositories.Fuseki;
using EngineeringSymbols.Api.Services.EngineeringSymbolService;
using EngineeringSymbols.Tools.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Annotations;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace EngineeringSymbols.Api.Infrastructure;

internal delegate Task<IResult> CreateEngineeringSymbolHandler(IEngineeringSymbolService symbolService, HttpRequest request, ClaimsPrincipal user, [FromQuery(Name = "validationOnly")] bool? validationOnly);

public static class EndpointsInfrastructure
{
    public const string SymbolTags = "Symbols";
    
    public static WebApplication AddEndpoints(this WebApplication app)
    {
        var symbols = app.MapGroup("/symbols");

        symbols.MapGet("/", async (IEngineeringSymbolService symbolService, ClaimsPrincipal user, bool? allVersions) 
            =>await symbolService
                    .GetSymbolsAsync(allVersions ?? false, publicVersion: !user.IsContributorOrAdmin())
                    .Match(TypedResults.Ok, exception => EndpointsCommon.OnFailure(exception, app.Logger)))
            .WithTags(SymbolTags)
            .WithMetadata(new SwaggerOperationAttribute("Get all Engineering Symbols", "Get all Engineering Symbols. If query parameter 'allVersions' is missing or 'false' only the latest version of a symbol is returned, otherwise all versions of every symbol is returned. Only published symbols will be returned for anonymous requests."))
            .Produces<List<EngineeringSymbolPublicDto>>()
            .Produces<List<EngineeringSymbolDto>>()
            .RequireRateLimiting(RateLimiterPolicy.Fixed)
            .AllowAnonymous();
        
        symbols.MapGet("/{idOrKey}", async (IEngineeringSymbolService symbolService, ClaimsPrincipal user, string idOrKey) 
                => await symbolService
                        .GetSymbolByIdOrKeyAsync(idOrKey, publicVersion: !user.IsContributorOrAdmin())
                        .Match(TypedResults.Ok, exception => EndpointsCommon.OnFailure(exception, app.Logger)))
            .WithTags(SymbolTags)
            .WithMetadata(new SwaggerOperationAttribute("Get an Engineering Symbol by Id or Key", "Get an Engineering Symbol by Id or Key. Only published symbols will be returned for anonymous requests."))
            .Produces<EngineeringSymbolPublicDto>()
            .Produces<EngineeringSymbolDto>()
            .RequireRateLimiting(RateLimiterPolicy.Fixed)
            .AllowAnonymous();
        
        
        symbols.MapPost("/", (CreateEngineeringSymbolHandler) (async (symbolService, request, user, validationOnly) => 
            {
                var allowedContentTypes = new []{"image/svg+xml", "application/json"};

                if (!allowedContentTypes.Contains(request.ContentType))
                {
                    return TypedResults.BadRequest($"Unsupported Content-Type. Expected ${string.Join(" or ", allowedContentTypes)}, but got {request.ContentType}");
                }
                
                const long maxSize = 500; // KiB
                
                if (request.ContentLength is <= 0 or > maxSize * 1024)
                    return TypedResults.BadRequest($"File size is 0 or greater than {maxSize} KiB");
                
                string content;
                
                using (var stream = new StreamReader(request.Body))
                {
                    content = await stream.ReadToEndAsync();
                }

                var cType = request.ContentType == "image/svg+xml"
                    ? CreateSymbol.InsertContentType.SvgString
                    : CreateSymbol.InsertContentType.Json;

                return await symbolService.CreateSymbolAsync(user, cType, content, validationOnly ?? false)
                    .Match(
                        Succ: guid => validationOnly is true ? TypedResults.Ok() : TypedResults.Created(guid), 
                        Fail: exception => EndpointsCommon.OnFailure(exception, app.Logger)
                        );
            }))
            .Accepts<string>( "image/svg+xml", "application/json")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status201Created)
            .WithTags(SymbolTags)
            .WithMetadata(new SwaggerOperationAttribute("Create an Engineering Symbol", "Create Engineering Symbol. If query parameter 'validationOnly' is true, only a validation of the SVG file is performed, nothing will be stored in the database."))
            .RequireAuthorization(Policy.ContributorOrAdmin);
        
        // Only for symbols that is in draft mode
        symbols.MapPut("/{id}", async (IEngineeringSymbolService symbolService, EngineeringSymbolCreateDto symbol) => Results.Problem("Endpoint not implemented"))
            .WithTags(SymbolTags)
            .WithMetadata(new SwaggerOperationAttribute("Update (replace) an Engineering Symbol by Id (When Status='Draft')", "Update (replace) an Engineering Symbol by Id. Note that this will only work if the symbol has Status='Draft'. The value of the 'Status' field is ignored and 'Id' and 'Key' must match existing entry."))
            .RequireAuthorization(Policy.ContributorOrAdmin);
        
        // Only for super admins
        // Only for existing status != "accepted"
        symbols.MapPut("/{id}/status", async (IEngineeringSymbolService symbolService, string id) => Results.Problem("Endpoint not implemented"))
            .WithTags(SymbolTags)
            .WithMetadata(new SwaggerOperationAttribute("Set the Status of an Engineering Symbol", "Set the Status of an Engineering Symbol"))
            .RequireAuthorization(Policy.OnlyAdmins);
        
        symbols.MapDelete("/{id}", async (IEngineeringSymbolService symbolService, string id)
                => await symbolService
                    .DeleteSymbolAsync(id)
                    .Match(Succ: _ => TypedResults.NoContent(), Fail: exception =>  EndpointsCommon.OnFailure(exception, app.Logger)))
            .WithTags(SymbolTags)
            .WithMetadata(new SwaggerOperationAttribute("Delete an Engineering Symbol", "Delete an Engineering Symbol"))
            .RequireAuthorization(Policy.OnlyAdmins);
        
        
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