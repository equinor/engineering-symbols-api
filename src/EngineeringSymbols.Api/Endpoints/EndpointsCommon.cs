using EngineeringSymbols.Tools.SvgParser.Models;

namespace EngineeringSymbols.Api.Endpoints;

/// <summary>
/// Common/generic methods for endpoints
/// </summary>
public static class Common
{
    public static IResult OnFailure(Exception exception)
    {
        // TODO: Log stuff
        return exception.Match<IResult>()
            .With<ValidationException>(ex =>
                TypedResults.ValidationProblem(ex.Errors, ex.Message, title: "Validation Error"))
            .With<SvgParseException>(ex =>
                TypedResults.Problem(ex.Message, title: "SVG Parse Error", statusCode: StatusCodes.Status400BadRequest))
            .With<RepositoryException>(ex => 
                TypedResults.Problem(ex.Message, title: "DB Error", statusCode: StatusCodes.Status400BadRequest))
            .Otherwise(_ =>
                TypedResults.Problem("Unexpected error", statusCode: StatusCodes.Status500InternalServerError));
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