namespace EngineeringSymbols.Tools.Models;

public static class ValidationExceptionResult
{
    public static Result<T> New<T>(string message, IDictionary<string, string[]>? errors = null, Exception? inner = null)
    {
        return new Result<T>(new ValidationException(message, errors, inner));
    }
}