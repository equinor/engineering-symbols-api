namespace EngineeringSymbols.Tools.SvgParser.Models;

public class SvgParseException : Exception
{
    public SvgParseException()
    {
        
    }
    
    public SvgParseException(string message) : base(message)
    {
        
    }
    
    public SvgParseException(string message, Exception inner) : base(message, inner)
    {
        
    }
}