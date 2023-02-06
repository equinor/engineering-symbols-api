using EngineeringSymbols.Tools.SvgParser;
using EngineeringSymbols.Tools.SvgParser.Models;

namespace EngineeringSymbols.Api.Endpoints;

public static class UploadEngineeringSymbol
{
    public static async Task<IResult> Endpoint(IFormFile file, IEngineeringSymbolService symbolService) =>
        await CreateUploadContext(file)
            .Bind(ReadFileToString)
            .Bind(ParseSvgString)
            .Bind(Save(symbolService))
            .Match(
                Succ: ctx => TypedResults.Ok(ctx.EngineeringSymbolDto),
                Fail: Common.OnFailure);
    
    private static TryAsync<UploadContext> CreateUploadContext(IFormFile file) => 
        TryAsync(() =>
        {
            var fileId = file.FileName;

            if(!fileId.Split(".").Apply(s => s.Length > 0 && s.Last().ToLower() == "svg"))
                throw new ValidationException("Only SVG files are supported");
            
            return Task.FromResult(new UploadContext(file) {FileId = fileId});
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
            SvgParser.FromString(
                    ctx.FileContent, 
                    opt => 
                    { 
                        opt.IncludeSvgString = false; 
                        opt.SymbolId = ctx.FileId; 
                    })
                .Match(
                    Succ: result => 
                    { 
                        if (result.ParseErrors.Count > 0)
                        {
                            throw new ValidationException(result.ParseErrors.Fold(
                                new Dictionary<string, string[]>(), (map, pair) =>
                                {
                                    map.Add(pair.Key, pair.Value.ToArray());
                                    return map;
                                }));
                        }
                        
                        if (result.EngineeringSymbol == null)
                        {
                            throw new ValidationException("SVG parse error");
                        }
                    
                        return Task.FromResult(ctx with { EngineeringSymbolDto = result.EngineeringSymbol.ToDto() }); 
                    }, 
                    Fail: ex => throw ex));
  
    
    private static Func<UploadContext, TryAsync<UploadContext>> Save(IEngineeringSymbolService symbolService) => 
        ctx => TryAsync(
            async () => await symbolService.SaveSymbolAsync(ctx.EngineeringSymbolDto)
                .Match(
                    Some: dto => ctx with { EngineeringSymbolDto = dto },
                    None: () => throw new ValidationException("Failed to read file contents")));
            
    
    private record UploadContext(IFormFile File)
    {
        public string FileId { get; init; }
        public string FileContent { get; init; }
        public EngineeringSymbolDto EngineeringSymbolDto { get; init; }
        public string CreatedUri { get; init; } 
    }
}