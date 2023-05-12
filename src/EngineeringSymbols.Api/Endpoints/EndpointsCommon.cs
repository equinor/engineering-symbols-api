using EngineeringSymbols.Tools.Models;
using EngineeringSymbols.Tools.SvgParser.Models;

namespace EngineeringSymbols.Api.Endpoints;

/// <summary>
/// Common/generic methods for endpoints
/// </summary>
public static class EndpointsCommon
{
    public static IResult OnFailure(Exception exception, ILogger? logger = null)
    {
        return exception.Match<IResult>()
            .With<ValidationException>(ex =>
                TypedResults.ValidationProblem(ex.Errors, ex.Message, title: "Validation Error"))
            .With<SvgParseException>(ex =>
                TypedResults.Problem(ex.Message, title: "SVG Parse Error", statusCode: StatusCodes.Status400BadRequest))
            .With<RepositoryException>(OnRepositoryException)
            .Otherwise(ex =>
            {
                logger?.LogError("Status500InternalServerError with exception: {Exception}", ex);
                return TypedResults.Problem("Unexpected Error", statusCode: StatusCodes.Status500InternalServerError);
            });
    }

    private static IResult OnRepositoryException(RepositoryException ex)
    {
        switch (ex.RepositoryOperationError)
        {
            case RepositoryOperationError.EntityNotFound:
                return TypedResults.NotFound();
            case RepositoryOperationError.EntityAlreadyExists:
                return TypedResults.UnprocessableEntity("Entity already exists");
            case RepositoryOperationError.Unknown:
            default:
                return TypedResults.Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError, title: "Repository Error");
        }
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    /// <exception cref="ValidationException"></exception>
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
        catch (Exception)
        {
            // TODO: log ex here?
            throw new ValidationException("Failed to read file contents");
        }

        return result;
    }
}