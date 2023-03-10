namespace EngineeringSymbols.Tools.Models;

public class ValidationException : Exception
{
    private const string _DefaultMessage = "One or more input parameters is invalid";
    public new string Message { get; init; }
    
    public IDictionary<string, string[]>? Errors { get; } = new Dictionary<string, string[]>();
    
    
    public ValidationException()
    {
        Message = _DefaultMessage;
    }
    
    public ValidationException(string message) //: base(message)
    {
        Message = message;
    }
    
    public ValidationException(IDictionary<string, string[]> errors)
    {
        Message = _DefaultMessage;
        Errors = errors;
    }
    
    public ValidationException(string message, IDictionary<string, string[]>? errors) // : base(message)
    {
        Message = message;
        Errors = errors;
    }
    
    public ValidationException(string message, IDictionary<string, string[]>? errors, Exception? inner) : base(message, inner)
    {
        Message = message;
        Errors = errors;
    }
    
    
    public ValidationException(string message, Exception? inner) : base(message, inner)
    {
        Message = message;
    }
    

}