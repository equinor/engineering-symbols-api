namespace EngineeringSymbols.Tools.SvgParser.Models;

public class SvgParseErrorException : Exception
{
    public SvgParseErrorException()
    {
        
    }
    
    public SvgParseErrorException(string message) : base(message)
    {
        
    }
    
    public SvgParseErrorException(string message, Exception inner) : base(message, inner)
    {
        
    }
}