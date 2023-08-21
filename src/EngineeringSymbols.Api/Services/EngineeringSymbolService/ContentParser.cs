using EngineeringSymbols.Tools;
using EngineeringSymbols.Tools.Models;
using EngineeringSymbols.Tools.SvgParser;
using EngineeringSymbols.Tools.Validation;
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

            Either<Exception, EngineeringSymbolCreateDto> parsedDto = Left(new Exception("Failed to deserialize symbol content"));
            
            switch (contentType)
            {
                case ContentTypes.Json:
                {
                    try
                    {
                        var obj = JsonConvert.DeserializeObject<EngineeringSymbolCreateDto>(content);

                        if (obj != null)
                            parsedDto = obj;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        parsedDto = new ValidationException($"Failed to deserialize JSON content.");
                    }

                    break;
                }
                case ContentTypes.Svg:
                    parsedDto = SvgParser.FromString(content)
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
                    break;
                default:
                    parsedDto = new ValidationException("Failed to deserialize symbol content. Invalid Content-Type");
                    break;
            }
        
            return parsedDto.Match<Either<Exception,EngineeringSymbolCreateDto>>(dto =>
                {
                    var validationResult = new EngineeringSymbolCreateDtoValidator().Validate(dto);

                    return validationResult.IsValid 
                        ? dto
                        : new ValidationException(validationResult.ToDictionary());
                },
                ex => ex);
    }
}