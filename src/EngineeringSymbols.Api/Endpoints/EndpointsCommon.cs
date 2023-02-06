using EngineeringSymbols.Tools.SvgParser.Models;

namespace EngineeringSymbols.Api.Endpoints;

/// <summary>
/// Common/generic methods for endpoints
/// </summary>
public static class Common
{
    public static IResult OnFailure(Exception ex)
    {
        if (ex is ValidationException validationException)
        {
            return TypedResults.ValidationProblem(validationException.Errors, validationException.Message, title: "Validation Error");
        }
        
        if (ex is SvgParseException svgParseException)
        {
            return TypedResults.Problem(svgParseException.Message, title: "SVG File Error",statusCode: StatusCodes.Status400BadRequest);
        }

        // TODO: Log 'ex'
        return TypedResults.Problem("Unexpected error", statusCode: StatusCodes.Status500InternalServerError);
    }
    
    public static async Task<string> ReadFileContentToString(IFormFile file)
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
}