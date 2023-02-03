namespace EngineeringSymbols.Tools.SvgParser.Models;

public record SvgParserResult
{
    public EngineeringSymbol? EngineeringSymbol { get; }
    
    public bool IsSuccess => ParseErrors.Count == 0 && EngineeringSymbol != null;
    public List<string> ParseErrors { get; } = new();

    public SvgParserResult(EngineeringSymbol symbol)
    {
	    EngineeringSymbol = symbol;
    }
    
    public SvgParserResult(List<string> parseErrors)
    {
	    ParseErrors = parseErrors;
    }
    
    public SvgParserResult(EngineeringSymbol symbol, List<string> parseErrors)
    {
	    EngineeringSymbol = symbol;
	    ParseErrors = parseErrors;
    }
	
}