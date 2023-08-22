using System.Security.Claims;
using EngineeringSymbols.Api.Endpoints;
using EngineeringSymbols.Api.Infrastructure.Auth;
using EngineeringSymbols.Api.Repositories.Fuseki;
using EngineeringSymbols.Api.Services.EngineeringSymbolService;
using EngineeringSymbols.Tools.Models;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;
using static EngineeringSymbols.Api.Endpoints.EndpointsCommon;

namespace EngineeringSymbols.Api.Infrastructure;

internal delegate Task<IResult> CreateEngineeringSymbolHandler(IEngineeringSymbolService symbolService,
    HttpRequest request, ClaimsPrincipal user, [FromQuery(Name = "validationOnly")] bool? validationOnly);

public static class EndpointsInfrastructure
{
    public const string SymbolTagsPublic = "Anonymous";
    public const string SymbolTagsManagement = "Authorization";

    public static Func<Exception, IResult> OnFail(ILogger logger) => exception => OnFailure(exception, logger);

    public static WebApplication AddEndpoints(this WebApplication app)
    {
        var anonymous = app.MapGroup("/symbols");

        anonymous.MapGet("/", async (IEngineeringSymbolService symbolService, bool? allVersions)
                => await symbolService
                    .GetSymbolsAsync(allVersions ?? false)
                    .Match(TypedResults.Ok, OnFail(app.Logger)))
            .WithTags(SymbolTagsPublic)
            .WithMetadata(new SwaggerOperationAttribute("Get all published Engineering Symbols",
                "Get all published Engineering Symbols. If query parameter 'allVersions' is missing or 'false' only the latest version of a symbol is returned, otherwise all versions of every symbol is returned. Only published symbols will be returned for anonymous requests."))
            .Produces<List<EngineeringSymbolPublicDto>>()
            .RequireRateLimiting(RateLimiterPolicy.Fixed)
            .AllowAnonymous();

        anonymous.MapGet("/{idOrKey}",
                async (IEngineeringSymbolService symbolService, ClaimsPrincipal user, string idOrKey)
                    => await symbolService
                        .GetSymbolByIdOrKeyAsync(idOrKey, publicVersion: !user.IsContributorOrAdmin())
                        .Match(TypedResults.Ok, OnFail(app.Logger)))
            .WithTags(SymbolTagsPublic)
            .WithMetadata(new SwaggerOperationAttribute("Get a published Engineering Symbol by Id or Key",
                "Get a published Engineering Symbol by Id or Key. All versions are returned if Key is specified. Only published symbols will be returned for anonymous requests."))
            .Produces<List<EngineeringSymbolPublicDto>>()
            .RequireRateLimiting(RateLimiterPolicy.Fixed)
            .AllowAnonymous();

        var management = app.MapGroup("manage/symbols");

        management.MapGet("/", async (IEngineeringSymbolService symbolService, bool? allVersions)
                => await symbolService
                    .GetSymbolsAsync(allVersions ?? false, publicVersion: false)
                    .Match(TypedResults.Ok, OnFail(app.Logger)))
            .WithTags(SymbolTagsManagement)
            .WithMetadata(new SwaggerOperationAttribute("Get all Engineering Symbols",
                "Get all Engineering Symbols. If query parameter 'allVersions' is missing or 'false' only the latest version of a symbol is returned, otherwise all versions of every symbol is returned."))
            .Produces<List<EngineeringSymbolDto>>()
            .RequireAuthorization(Policy.ContributorOrAdmin);

        management.MapGet("/{idOrKey}",
                async (IEngineeringSymbolService symbolService, ClaimsPrincipal user, string idOrKey)
                    => await symbolService
                        .GetSymbolByIdOrKeyAsync(idOrKey, publicVersion: !user.IsContributorOrAdmin())
                        .Match(TypedResults.Ok, OnFail(app.Logger)))
            .WithTags(SymbolTagsManagement)
            .WithMetadata(new SwaggerOperationAttribute("Get an Engineering Symbol by Id or Key",
                "Get an Engineering Symbol by Id or Key. All versions are returned if Key is specified."))
            .Produces<List<EngineeringSymbolDto>>()
            .RequireAuthorization(Policy.ContributorOrAdmin);

        management.MapPost("/",
                (CreateEngineeringSymbolHandler) (async (symbolService, request, user, validationOnly) =>
                {
                    var allowedContentTypes = new[] {ContentTypes.Json, ContentTypes.Svg};

                    if (request.ContentType is null || !allowedContentTypes.Contains(request.ContentType))
                    {
                        return TypedResults.BadRequest(
                            $"Unsupported Content-Type. Expected ${string.Join(" or ", allowedContentTypes)}, but got {request.ContentType}");
                    }

                    var userId = user.Identity?.Name;

                    if (userId is null)
                    {
                        return TypedResults.BadRequest("Failed to determine UserId");
                    }

                    string content;

                    using (var stream = new StreamReader(request.Body))
                    {
                        content = await stream.ReadToEndAsync();
                    }

                    return await ContentParser.ParseSymbolCreateContent(request.ContentType, content)
                        .MatchAsync(
                            RightAsync: async dto => await symbolService
                                .CreateSymbolAsync(dto with {Owner = userId}, validationOnly ?? false)
                                .Match(
                                    Succ: guid =>
                                        validationOnly is true ? TypedResults.Ok() : TypedResults.Created(guid),
                                    Fail: OnFail(app.Logger))
                            , Left: OnFail(app.Logger));
                }))
            .Accepts<string>(ContentTypes.Svg, ContentTypes.Json)
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status201Created)
            .WithTags(SymbolTagsManagement)
            .WithMetadata(new SwaggerOperationAttribute("Create an Engineering Symbol revision",
                "Create Engineering Symbol revision. If query parameter 'validationOnly' is true, only a validation of the SVG file is performed, nothing will be stored in the database."))
            .RequireAuthorization(Policy.ContributorOrAdmin);

        // Only for symbols that is in draft mode
        management.MapPut("/{id}",
                async (IEngineeringSymbolService symbolService, string id, EngineeringSymbolCreateDto createDto)
                    => await symbolService.UpdateSymbolAsync(id, createDto).Match(
                        Succ: success => success ? TypedResults.Ok() : TypedResults.Problem("Updated failed"),
                        Fail: OnFail(app.Logger)))
            .WithTags(SymbolTagsManagement)
            .WithMetadata(new SwaggerOperationAttribute(
                "Update (replace) an Engineering Symbol revision by Id (Only when Status='Draft')",
                "Update (replace) an Engineering Symbol by Id. Note that this will only work if the symbol has Status='Draft'. The value of the 'Status' field is ignored and 'Id' and 'Key' must match existing entry."))
            .RequireAuthorization(Policy.ContributorOrAdmin);

        // Only for super admins
        // Only for symbols with status != "Published"
        management.MapPut("/{id}/status",
                async (IEngineeringSymbolService symbolService, string id, EngineeringSymbolStatusDto statusDto)
                    => await symbolService.UpdateSymbolStatusAsync(id, statusDto)
                        .Match(
                            Succ: success => success ? TypedResults.Ok() : TypedResults.Problem("Updated failed"),
                            Fail: OnFail(app.Logger)))
            .WithTags(SymbolTagsManagement)
            .WithMetadata(new SwaggerOperationAttribute("Set the Status of an Engineering Symbol revision",
                "Set the Status of an Engineering Symbol"))
            .RequireAuthorization(Policy.OnlyAdmins);

        management.MapDelete("/{id}", async (IEngineeringSymbolService symbolService, string id)
                => await symbolService
                    .DeleteSymbolAsync(id)
                    .Match(_ => TypedResults.NoContent(), OnFail(app.Logger)))
            .WithTags(SymbolTagsManagement)
            .WithMetadata(new SwaggerOperationAttribute("Delete an Engineering Symbol revision",
                "Delete an Engineering Symbol"))
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