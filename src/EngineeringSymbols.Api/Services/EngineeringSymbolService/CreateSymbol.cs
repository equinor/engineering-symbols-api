using System.Security.Claims;
using System.Security.Principal;
using EngineeringSymbols.Tools.Models;
using EngineeringSymbols.Tools.SvgParser;
using Microsoft.AspNetCore.Mvc;

namespace EngineeringSymbols.Api.Services.EngineeringSymbolService;

public static class CreateSymbol
{
    public record InsertContext(IFormFile File, string Key, string User)
    {
        public string FileId { get; init; } = "Unknown";
        public string FileContent { get; init; } = "";
        public EngineeringSymbolCreateDto? EngineeringSymbolCreateDto { get; init; }
        
        public string CreatedUri { get; init; } = "UnknownUri";
    }
    
    public static TryAsync<InsertContext> CreateInsertContext(IPrincipal user, IFormFile file) => 
        TryAsync(() =>
        {
            var fileId = file.FileName;
            
            var key = Path.GetFileNameWithoutExtension(fileId);
            var extension = Path.GetExtension(fileId);
            
            var userId = user.Identity?.Name;

            if (userId == null)
            {
                throw new ValidationException("UserId was null");
            }

            if (!string.Equals(extension, ".svg", StringComparison.OrdinalIgnoreCase))
            {
                throw new ValidationException("Only SVG files are supported");
            }

            return Task.FromResult(new InsertContext(file, key, userId) {FileId = fileId, Key = key});
        });
    
    public static TryAsync<InsertContext> ReadFileToString(InsertContext ctx) => 
        TryAsync(async () =>
        {
            const long maxSize = 500; // KiB
            
            var length = ctx.File.Length;
            if (length is <= 0 or > maxSize * 1024)
                throw new ValidationException($"File size is 0 or greater than {maxSize} KiB");

            var fileContent = await ReadFileContentToString(ctx.File);
            
            return ctx with { FileContent = fileContent };
        });

    public static TryAsync<InsertContext> ParseSvgString(InsertContext ctx) =>
        TryAsync(() =>
            SvgParser.FromString(ctx.FileContent)
                .Match(
                    Succ: result => 
                    { 
                        if (result.ParseErrors.Count > 0)
                            throw new ValidationException(
                                result.ParseErrors.ToDictionary(pair => pair.Key, pair => pair.Value.ToArray()));
                        
                        if (result.EngineeringSymbolSvgParsed == null)
                            throw new ValidationException("SVG parse error");
                        
                        return Task.FromResult(ctx with
                        {
                            EngineeringSymbolCreateDto = result.EngineeringSymbolSvgParsed
                                .ToCreateDto(ctx.Key, ctx.User, "", ctx.FileId)
                        });
                    }, 
                    Fail: exception => throw exception));
    
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
        catch (Exception)
        {
            // TODO: log ex here?
            throw new ValidationException("Failed to read file contents");
        }

        return result;
    }
}