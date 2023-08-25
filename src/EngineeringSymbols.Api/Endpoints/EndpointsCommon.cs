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
                TypedResults.Problem(ex.Message, title: "SVG Parse Error"))
            .With<RepositoryException>(ex => OnRepositoryException(ex, logger))
            .With<BadHttpRequestException>(ex =>
                TypedResults.Problem(ex.Message, statusCode: ex.StatusCode))
            .Otherwise(ex =>
            {
                logger?.LogError("Status500InternalServerError with exception: {Exception}", ex);

                var det = ex.Message + ". Inner ex: " + ex.InnerException?.Message;
                throw ex;
                return TypedResults.Problem("Unexpected Error. " + det, statusCode: StatusCodes.Status500InternalServerError);
            });
    }

    private static IResult OnRepositoryException(RepositoryException ex, ILogger? logger = null)
    {
        switch (ex.RepositoryOperationError)
        {
            case RepositoryOperationError.EntityNotFound:
                return TypedResults.NotFound();
            case RepositoryOperationError.EntityAlreadyExists:
                return TypedResults.UnprocessableEntity("Entity already exists");
            case RepositoryOperationError.UpdateFailed:
            case RepositoryOperationError.Unknown:
            default:
                logger?.LogError("Unknown RepositoryOperationError with exception: {Exception}", ex);
                return TypedResults.Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError, title: "Repository Error");
        }
    }
}