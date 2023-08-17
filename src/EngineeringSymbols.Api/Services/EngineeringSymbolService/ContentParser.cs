using EngineeringSymbols.Tools;
using EngineeringSymbols.Tools.Models;
using EngineeringSymbols.Tools.SvgParser;
using Newtonsoft.Json;

namespace EngineeringSymbols.Api.Services.EngineeringSymbolService;

public static class ContentParser
{
    public static Either<Exception,EngineeringSymbolCreateDto> ParseSymbolCreateContent(string? contentType, string? content)
    {
            if (content is null || contentType is null)
            {
                return new ValidationException("Failed to deserialize symbol content.");
            }

            const double maxSize = 500; // KiB
            var byteSize = content.Length * 2; // Approx
            var sizeInKiB = (double) byteSize / 1024;

            if (sizeInKiB is > maxSize or 0)
            {
                return new ValidationException($"Content size is 0 or greater than {maxSize} KiB");
            }

            switch (contentType)
            {
                case ContentTypes.Json:
                {
                    EngineeringSymbolCreateDto? dto = null;

                    try
                    {
                        dto = JsonConvert.DeserializeObject<EngineeringSymbolCreateDto>(content);
                        //dto = JsonSerializer.Deserialize<EngineeringSymbolCreateDto>(ctx.Content);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        return new ValidationException($"Failed to deserialize JSON content.");
                    }
                    
                    return dto;
                }
                case ContentTypes.Svg:
                    return SvgParser.FromString(content)
                        .Match<Either<Exception,EngineeringSymbolCreateDto>>(
                            Succ: result =>
                            {
                                if (result.ParseErrors.Count > 0)
                                    return new ValidationException(
                                        result.ParseErrors.ToDictionary(pair => pair.Key,
                                            pair => pair.Value.ToArray()));

                                if (result.EngineeringSymbolSvgParsed == null)
                                    return new ValidationException("SVG parse error");

                                return result.EngineeringSymbolSvgParsed.ToCreateDto("");
                            },
                            Fail: exception => exception);
                default:
                    return new ValidationException("Failed to deserialize symbol content. Invalid Content-Type");
            }
    }
}