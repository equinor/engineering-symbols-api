using EngineeringSymbols.Api.Entities;
using EngineeringSymbols.Tools.Models;
using EngineeringSymbols.Tools.SvgParser;
using EngineeringSymbols.Tools.SvgParser.Models;

namespace EngineeringSymbols.Api.Endpoints;

public static class UploadEngineeringSymbol
{
    public static async Task<IResult> UploadAsync(HttpContext httpContext, IFormFile file, IEngineeringSymbolService symbolService) =>
        await CreateUploadContext(httpContext, file)
            .Bind(ReadFileToString)
            .Bind(ParseSvgString)
            .Bind(Save(symbolService))
            .Match(
                Succ: ctx => TypedResults.Created(ctx.CreatedUri, ctx.EngineeringSymbolCompleteDto),
                Fail: Common.OnFailure);
    
    private static TryAsync<UploadContext> CreateUploadContext(HttpContext httpContext, IFormFile file) => 
        TryAsync(() =>
        {
            var fileId = file.FileName;
            var user = httpContext.User.Identity?.Name ?? "Maverick";
            
            if(!fileId.Split(".").Apply(s => s.Length > 0 && s.Last().ToLower() == "svg"))
                throw new ValidationException("Only SVG files are supported");
            
            return Task.FromResult(new UploadContext(file, user) {FileId = fileId});
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
                    opt.IncludeSvgString = true;
                    opt.UseSymbolId = ctx.FileId;
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
                            EngineeringSymbolCreateDto = result.EngineeringSymbolParsed.ToCreateDto(ctx.User)
                        }); 
                    }, 
                    Fail: ex => throw ex));


    private static Func<UploadContext, TryAsync<UploadContext>> Save(IEngineeringSymbolService symbolService) =>
        ctx => symbolService.CreateSymbolAsync(ctx.EngineeringSymbolCreateDto)
                .Map(symbol => ctx with
                {
                    EngineeringSymbolCompleteDto = symbol.ToCompleteResponseDto(),
                    CreatedUri = symbol.Id
                });

    
    private record UploadContext(IFormFile File, string User)
    {
        public string FileId { get; init; }
        public string FileContent { get; init; }
        public EngineeringSymbolCreateDto EngineeringSymbolCreateDto { get; init; }
        public EngineeringSymbolCompleteResponseDto EngineeringSymbolCompleteDto { get; init; }
        public string CreatedUri { get; init; } 
    }
}