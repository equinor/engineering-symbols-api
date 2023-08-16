using System.Security.Claims;
using System.Security.Principal;
using System.Text.Json;
using System.Text.Json.Serialization;
using EngineeringSymbols.Tools;
using EngineeringSymbols.Tools.Models;
using EngineeringSymbols.Tools.SvgParser;
using EngineeringSymbols.Tools.Validation;
using Newtonsoft.Json;


namespace EngineeringSymbols.Api.Services.EngineeringSymbolService;

public static class CreateSymbol
{

    public enum InsertContentType
    {
        SvgString,
        Json
    }
    
    public record InsertContext
    {
        public bool ValidationOnly { get; set; }
        public InsertContentType ContentType { get; set; }
        public string Content { get; init; }
        public string User { get; init; }
        public EngineeringSymbolCreateDto EngineeringSymbolCreateDto { get; init; }
        public EngineeringSymbolDto EngineeringSymbolDto { get; init; }
        public string CreatedUri { get; init; } = "UnknownUri";
    }

    public static TryAsync<InsertContext> CreateInsertContext(IPrincipal user, InsertContentType contentType, string content, bool validationOnly) => 
        TryAsync(() =>
        {
            var userId = user.Identity?.Name;

            if (userId == null)
            {
                throw new ValidationException("UserId was null");
            }
            
            return Task.FromResult(new InsertContext { User = userId, ContentType = contentType, Content = content, ValidationOnly = validationOnly} );
        });
    
    public static TryAsync<InsertContext> ParseContent(InsertContext ctx) => 
        TryAsync(() =>
        {
            switch (ctx.ContentType)
            {
                case InsertContentType.Json:
                {
                    EngineeringSymbolCreateDto? dto = null;
                    
                    try
                    {
                        dto = JsonConvert.DeserializeObject<EngineeringSymbolCreateDto>(ctx.Content);
                        //dto = JsonSerializer.Deserialize<EngineeringSymbolCreateDto>(ctx.Content);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
            
                    if (dto == null)
                    {
                        throw new ValidationException("Failed to deserialize JSON object.");
                    }

                    return Task.FromResult(ctx with { EngineeringSymbolCreateDto = dto });
                }
                case InsertContentType.SvgString:
                    return SvgParser.FromString(ctx.Content)
                        .Match(
                            Succ: result => 
                            { 
                                if (result.ParseErrors.Count > 0)
                                    throw new ValidationException(
                                        result.ParseErrors.ToDictionary(pair => pair.Key, pair => pair.Value.ToArray()));
                        
                                if (result.EngineeringSymbolSvgParsed == null)
                                    throw new ValidationException("SVG parse error");
                        
                                return Task.FromResult(ctx with { EngineeringSymbolCreateDto = result.EngineeringSymbolSvgParsed.ToCreateDto(ctx.User) });
                            }, 
                            Fail: exception => throw exception);
                default:
                    throw new ValidationException("Failed to parse symbol.");
            }
        });
    
    
    public static TryAsync<InsertContext> CreateInsertDto(InsertContext ctx) => 
        TryAsync(async () => ctx with {EngineeringSymbolDto = ctx.EngineeringSymbolCreateDto.ToDto()});
    
    
    public static TryAsync<InsertContext> ValidateEngineeringSymbol(InsertContext ctx) => 
        TryAsync(async () =>
        {

            ctx.EngineeringSymbolCreateDto.Validate()
                .IfFail(errors => throw new ValidationException(errors));
                
            return ctx;
        });
}