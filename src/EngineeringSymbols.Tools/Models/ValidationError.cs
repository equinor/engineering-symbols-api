namespace EngineeringSymbols.Tools.Models;

public class ValidationError : NewType<ValidationError, string>
{
    public string? Category { get; }
    
    public ValidationError(string e) : base(e)
    {
        
    }
    
    public ValidationError(string category, string e) : base(e)
    {
        Category = category;
    }
}