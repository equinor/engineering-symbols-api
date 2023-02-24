namespace EngineeringSymbols.Api.Models;

public class ValidationError : NewType<ValidationError, string>
{
    public string? Category { get; }
    
    public ValidationError(string e) : base(e)
    {
    }
    
    public ValidationError(string e, string category) : base(e)
    {
        Category = category;
    }
}