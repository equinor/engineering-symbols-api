using System.Text;
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
				var exceptionDetails = new StringBuilder();
				exceptionDetails.AppendLine($"An unhandled exception occured!");
				exceptionDetails.AppendLine($"Message: {ex.Message}");
				exceptionDetails.AppendLine($"Source: {ex.Source}");
				exceptionDetails.AppendLine($"StackTrace: {ex.StackTrace}");

				if (ex.InnerException != null)
				{
					exceptionDetails.AppendLine($"Inner Exception: {ex.InnerException}");
				}

				// TODO: Don't return error message to client in production!
				var message = exceptionDetails.ToString();

				logger?.LogError(message);

				return TypedResults.Problem(detail: message, statusCode: StatusCodes.Status500InternalServerError);
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