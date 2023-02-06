using EngineeringSymbols.Api.Models;
using EngineeringSymbols.Api.Services;
using EngineeringSymbols.Tools.Models;
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
                Fail: OnFailure);
    
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

            var fileContent = await ReadFileContentToString(ctx.File);
            
            return ctx with { FileContent = fileContent };
        });

    private static TryAsync<UploadContext> ParseSvgString(UploadContext ctx) 
        => TryAsync(() =>
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
                            throw new ValidationException("");
                        }
                    
                        return Task.FromResult(ctx with { EngineeringSymbolDto = result.EngineeringSymbol.ToDto() }); 
                    }, 
                    Fail: ex => throw ex)
            );
  
    
    private static Func<UploadContext, TryAsync<UploadContext>> Save(IEngineeringSymbolService symbolService) 
        => ctx => TryAsync(async () =>
        {
            await symbolService.SaveSymbol(ctx.EngineeringSymbolDto);
            return ctx with { };
        });

    private static async Task<string> ReadFileContentToString(IFormFile file)
    {
        string result;
        
        try
        {
            await using var fileStream = file.OpenReadStream();
            var bytes = new byte[file.Length];
            var a = await fileStream.ReadAsync(bytes, 0, (int)file.Length);
            result = System.Text.Encoding.UTF8.GetString(bytes);
        }
        catch (Exception _)
        {
            // TODO: log ex here?
            throw new ValidationException("Failed to read file contents");
        }

        return result;
    }
    
    private static IResult OnFailure(Exception ex)
    {
        if (ex is ValidationException validationException)
        {
            return TypedResults.ValidationProblem(validationException.Errors, validationException.Message, title: "SVG Validation Error");
        }
        
        if (ex is SvgParseException svgParseException)
        {
            return TypedResults.Problem(svgParseException.Message, title: "SVG File Error",statusCode: StatusCodes.Status400BadRequest);
        }

        // TODO: Log 'ex'
        return TypedResults.Problem("Unexpected error", statusCode: StatusCodes.Status500InternalServerError);
    }
}

public record UploadContext(IFormFile File)
{
    public string FileId { get; init; }
    public string FileContent { get; init; }
    public EngineeringSymbolDto EngineeringSymbolDto { get; init; }
    
    public string SymbolCreatedUrl { get; init; } 
}