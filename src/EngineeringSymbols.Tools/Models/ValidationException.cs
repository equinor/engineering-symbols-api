namespace EngineeringSymbols.Tools.Models;

public class ValidationException : Exception
{
    public IDictionary<string, string[]> Errors { get; } = new Dictionary<string, string[]>();
    
    public ValidationException()
    {
        
    }
    
    public ValidationException(string message): base(message)
    {

    }
    
    public ValidationException(IDictionary<string, string[]> errors)
    {
        Errors = errors;
    }
    
    public ValidationException(string message, IDictionary<string, string[]> errors) : base(message)
    {
        Errors = errors;
    }
    
    public ValidationException(string message, Exception inner) : base(message, inner)
    {
    }
}