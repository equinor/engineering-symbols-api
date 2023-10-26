using System.Security.Claims;
using EngineeringSymbols.Api.Infrastructure.Auth;
using EngineeringSymbols.Api.Repositories;
using EngineeringSymbols.Api.Repositories.Fuseki;
using EngineeringSymbols.Tools;
using EngineeringSymbols.Tools.Entities;
using EngineeringSymbols.Tools.Models;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using static EngineeringSymbols.Api.Endpoints.EndpointHelpers;
using static EngineeringSymbols.Api.Endpoints.EndpointsCommon;

namespace EngineeringSymbols.Api.Infrastructure;

internal delegate Task<IResult> CreateEngineeringSymbolHandler(IEngineeringSymbolService symbolService,
    HttpRequest request, ClaimsPrincipal claimsPrincipal, [FromQuery(Name = "validationOnly")] bool? validationOnly);

public static class EndpointsInfrastructure
{
    public const string SymbolTagsPublic = "Anonymous";
    public const string SymbolTagsManagement = "Authorization";

    public static Func<Exception, IResult> OnFail(ILogger logger) => exception => OnFailure(exception, logger);

    public static WebApplication AddEndpoints(this WebApplication app)
    {
        var anonymous = app.MapGroup("/symbols");

        anonymous.MapGet("/", async (IEngineeringSymbolService symbolService, bool? onlyLatestVersion)
                => await symbolService
                    .GetSymbolsAsync(onlyLatestVersion ?? true, publicVersion: true)
                    .Match(Results.Extensions.EngineeringSymbol, OnFail(app.Logger)))
            .WithTags(SymbolTagsPublic)
            .WithMetadata(new SwaggerOperationAttribute("Get all published Engineering Symbols",
                "Get all published Engineering Symbols. If query parameter 'allVersions' is missing or 'false' only the latest version of a symbol is returned, otherwise all versions of every symbol is returned. Only published symbols will be returned for anonymous requests."))
            .Produces<string>(contentType: ContentTypes.JsonLd)
            .Produces<string>(StatusCodes.Status400BadRequest, contentType: ContentTypes.Json)
            .RequireRateLimiting(RateLimiterPolicy.Fixed)
            .AllowAnonymous();


        anonymous.MapGet("/{idOrIdentifier}",
                async (IEngineeringSymbolService symbolService, ClaimsPrincipal user, string idOrIdentifier)
                    => await symbolService
                        .GetSymbolByIdOrIdentifierAsync(idOrIdentifier, publicVersion: true)
                        .Match(Results.Extensions.EngineeringSymbol, OnFail(app.Logger)))
            .WithTags(SymbolTagsPublic)
            .WithMetadata(new SwaggerOperationAttribute("Get a published Engineering Symbol by Id or Identifier",
                "Get a published Engineering Symbol by Id or Identifier. All versions are returned if Identifier is specified. Only published symbols will be returned for anonymous requests."))
            .Produces<string>(contentType: ContentTypes.JsonLd)
            .Produces<string>(StatusCodes.Status400BadRequest, contentType: ContentTypes.Json)
            .RequireRateLimiting(RateLimiterPolicy.Fixed)
            .AllowAnonymous();


        var management = app.MapGroup("manage/symbols");

        management.MapGet("/", async (IEngineeringSymbolService symbolService, bool? onlyLatestVersion)
                => await symbolService
                    .GetSymbolsAsync(onlyLatestVersion ?? true, publicVersion: false)
                    .Match(Results.Extensions.EngineeringSymbol, OnFail(app.Logger)))
            .WithTags(SymbolTagsManagement)
            .WithMetadata(new SwaggerOperationAttribute("Get all Engineering Symbols",
                "Get all Engineering Symbols. If query parameter 'allVersions' is missing or 'false' only the latest version of a symbol is returned, otherwise all versions of every symbol is returned."))
            .Produces<string>(contentType: ContentTypes.JsonLd)
            .Produces<string>(StatusCodes.Status400BadRequest, contentType: ContentTypes.Json)
            .RequireAuthorization(Policy.ContributorOrAdmin);
        
        management.MapGet("/{idOrIdentifier}",
                async (IEngineeringSymbolService symbolService, ClaimsPrincipal claimsPrincipal, string idOrIdentifier)
                    => await symbolService
                        .GetSymbolByIdOrIdentifierAsync(idOrIdentifier, publicVersion: false)
                        .Match(Results.Extensions.EngineeringSymbol, OnFail(app.Logger)))
            .WithTags(SymbolTagsManagement)
            .WithMetadata(new SwaggerOperationAttribute("Get an Engineering Symbol by Id or Identifier",
                "Get an Engineering Symbol by Id or Identifier. All versions are returned if Identifier is specified."))
            .Produces<string>(contentType: ContentTypes.JsonLd)
            .Produces<string>(StatusCodes.Status400BadRequest, contentType: ContentTypes.Json)
            .RequireAuthorization(Policy.ContributorOrAdmin);


        management.MapPost("/",
                (CreateEngineeringSymbolHandler)(async (symbolService, request, claimsPrincipal, validationOnly) =>
                    await GetSymbolCreateContentFromRequest(request)
                        .Bind(content => ParseSymbolCreateContent(request.ContentType, content))
                        .Bind(dto => AddUserFromClaimsPrincipal(dto, claimsPrincipal))
                        .Bind(ValidatePutDto)
                        .Bind(dto => symbolService.CreateSymbolAsync(dto, validationOnly ?? false))
                        .Match(
                            Succ: symbol => validationOnly is true ? TypedResults.Ok(symbol) : TypedResults.Created(symbol.Id, symbol),
                            Fail: OnFail(app.Logger))
                ))
            .Accepts<string>(ContentTypes.Svg, ContentTypes.Json)
            .Produces<EngineeringSymbolPutDto>(StatusCodes.Status200OK)
            .Produces<EngineeringSymbol>(StatusCodes.Status201Created)
            .WithTags(SymbolTagsManagement)
            .WithMetadata(new SwaggerOperationAttribute("Create an Engineering Symbol revision",
                "Create Engineering Symbol revision. If query parameter 'validationOnly' is true, only a validation of the SVG file or JSON symbol object is performed, nothing will be stored in the database."))
            .RequireAuthorization(Policy.ContributorOrAdmin);


        management.MapPut("/{id}",
                async (IEngineeringSymbolService symbolService, ClaimsPrincipal claimsPrincipal, string id, EngineeringSymbolPutDto putDto) =>
                    await ValidatePutDto(putDto)
                        .Bind(dto => AddUserFromClaimsPrincipal(dto, claimsPrincipal))
                        .Bind(dto => symbolService.UpdateSymbolAsync(id, dto))
                        .Match(Succ: TypedResults.Ok, Fail: OnFail(app.Logger)))
            .WithTags(SymbolTagsManagement)
            .WithMetadata(new SwaggerOperationAttribute(
                "Update (replace) an Engineering Symbol revision by Id (Only when Status='Draft')",
                "Update (replace) an Engineering Symbol by Id. Note that this will only work if the symbol has Status='Draft'."))
            .RequireAuthorization(Policy.ContributorOrAdmin);


        management.MapPut("/{id}/status",
                async (IEngineeringSymbolService symbolService, string id, EngineeringSymbolStatusDto statusDto)
                    => await symbolService.UpdateSymbolStatusAsync(id, statusDto)
                        .Match(
                            Succ: _ => TypedResults.Ok(),
                            Fail: OnFail(app.Logger)))
            .WithTags(SymbolTagsManagement)
            .WithMetadata(new SwaggerOperationAttribute("Set the Status of an Engineering Symbol revision",
                "Set the Status of an Engineering Symbol revision"))
            .RequireAuthorization(Policy.OnlyAdmins);


        management.MapDelete("/{id}", async (IEngineeringSymbolService symbolService, string id)
                => await symbolService
                    .DeleteSymbolAsync(id)
                    .Match(_ => TypedResults.NoContent(), OnFail(app.Logger)))
            .WithTags(SymbolTagsManagement)
            .WithMetadata(new SwaggerOperationAttribute("Delete an Engineering Symbol revision",
                "Delete an Engineering Symbol revision"))
            .RequireAuthorization(Policy.OnlyAdmins);

        var fuseki = app.MapGroup("/fuseki");

        fuseki.MapPost("/query", async (HttpRequest request, IEngineeringSymbolRepository repo)
                => await GetRequestBodyAsString(request)
                    .Bind(query => repo.FusekiQueryAsync(query, request.Headers.Accept.ToString()))
                    .Match(Results.Extensions.Fuseki, OnFail(app.Logger)))
            .WithTags(SymbolTagsManagement)
            .WithMetadata(new SwaggerOperationAttribute("Fuseki query", "Query fuseki"))
            .Accepts<string>("application/sparql-query; charset=UTF-8")
            .Produces<FusekiSelectResponse>(contentType: ContentTypes.JsonLd,
                additionalContentTypes: ContentTypes.Turtle)
            .RequireAuthorization(Policy.OnlyAdmins);

        fuseki.MapPost("/update", async (HttpRequest request, IEngineeringSymbolRepository repo)
                => await GetRequestBodyAsString(request)
                    .Bind(query => repo.FusekiUpdateAsync(query, request.Headers.Accept.ToString()))
                    .Match(Results.Extensions.Fuseki, OnFail(app.Logger)))
            .WithTags(SymbolTagsManagement)
            .WithMetadata(new SwaggerOperationAttribute("Fuseki UPDATE query", "Update Query"))
            .Accepts<string>("application/sparql-query; charset=UTF-8")
            .Produces<string>(contentType: ContentTypes.JsonLd,
                additionalContentTypes: ContentTypes.Turtle)
            .RequireAuthorization(Policy.OnlyAdmins);

        return app;
    }
}