namespace EngineeringSymbols.Tools.Models;

public class ValidationException : Exception
{
    public string? Message { get; set; }
    public IDictionary<string, string[]> Errors { get; } = new Dictionary<string, string[]>();
    
    public ValidationException()
    {
        
    }
    
    public ValidationException(string message) //: base(message)
    {
        Message = message;
    }
    
    public ValidationException(IDictionary<string, string[]> errors)
    {
        Errors = errors;
    }
    
    public ValidationException(string message, IDictionary<string, string[]> errors) //: base(message)
    {
        Message = message;
        Errors = errors;
    }
    
    public ValidationException(string message, Exception inner) : base(null, inner)
    {
        Message = message;
    }
}