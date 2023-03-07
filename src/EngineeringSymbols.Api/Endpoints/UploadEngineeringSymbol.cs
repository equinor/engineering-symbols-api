using System.Security.Claims;
using System.Security.Principal;
using EngineeringSymbols.Tools.Models;
using EngineeringSymbols.Tools.SvgParser;
using Microsoft.AspNetCore.Mvc;

namespace EngineeringSymbols.Api.Endpoints;

public static class UploadEngineeringSymbol
{
    public static async Task<IResult> UploadAsync(ClaimsPrincipal user, [FromForm] IFormFile svgFile, [FromServices] IEngineeringSymbolService symbolService) =>
        await CreateUploadContext(user, svgFile)
            .Bind(ReadFileToString)
            .Bind(ParseSvgString)
            .Bind(Save(symbolService))
            .Match(
                Succ: ctx => TypedResults.Created(ctx.CreatedUri),
                Fail: Common.OnFailure);
    
    private static TryAsync<UploadContext> CreateUploadContext(IPrincipal user, IFormFile file) => 
        TryAsync(() =>
        {
            var fileId = file.FileName;
            var userId = user.Identity?.Name ?? "Maverick";
            
            if(!fileId.Split(".").Apply(s => s.Length > 0 && s.Last().ToLower() == "svg"))
                throw new ValidationException("Only SVG files are supported");
            
            return Task.FromResult(new UploadContext(file, userId) {FileId = fileId});
        });
    
    private static TryAsync<UploadContext> ReadFileToString(UploadContext ctx) => 
        TryAsync(async () =>
        {
            const long maxSize = 500; // KiB
            
            var length = ctx.File.Length;
            if (length is <= 0 or > maxSize * 1024)
                throw new ValidationException($"File size is 0 or greater than {maxSize} KiB");

            var fileContent = await Common.ReadFileContentToString(ctx.File);
            
            return ctx with { FileContent = fileContent };
        });

    private static TryAsync<UploadContext> ParseSvgString(UploadContext ctx) =>
        TryAsync(() =>
            SvgParser.FromString(ctx.FileContent, opt =>
                {
                    opt.RemoveAnnotations = true;
                })
                .Match(
                    Succ: result => 
                    { 
                        if (result.ParseErrors.Count > 0)
                            throw new ValidationException(
                                result.ParseErrors.ToDictionary(pair => pair.Key, pair => pair.Value.ToArray()));
                        
                        if (result.EngineeringSymbolParsed == null)
                            throw new ValidationException("SVG parse error");
                        
                        return Task.FromResult(ctx with
                        {
                            EngineeringSymbolCreateDto = result.EngineeringSymbolParsed.ToCreateDto(ctx.User, ctx.FileId)
                        }); 
                    }, 
                    Fail: exception => throw exception));


    private static Func<UploadContext, TryAsync<UploadContext>> Save(IEngineeringSymbolService symbolService) =>
        ctx => symbolService.CreateSymbolAsync(ctx.EngineeringSymbolCreateDto!)
                .Map(symbolId => ctx with {CreatedUri = symbolId} );

    
    private record UploadContext(IFormFile File, string User)
    {
        public string FileId { get; init; } = "Unknown";
        public string FileContent { get; init; } = "";
        public EngineeringSymbolCreateDto? EngineeringSymbolCreateDto { get; init; }
        public string CreatedUri { get; init; } = "UnknownUri";
    }
}