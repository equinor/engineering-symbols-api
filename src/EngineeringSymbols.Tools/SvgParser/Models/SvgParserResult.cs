namespace EngineeringSymbols.Tools.SvgParser.Models;

public record SvgParserResult
{
    public EngineeringSymbol? EngineeringSymbol { get; }
    
    public bool IsSuccess => ParseErrors.Count == 0 && EngineeringSymbol != null;
    public Dictionary<string, List<string>> ParseErrors { get; } = new();

    public SvgParserResult(EngineeringSymbol symbol)
    {
	    EngineeringSymbol = symbol;
    }
    
    public SvgParserResult(Dictionary<string, List<string>> parseErrors)
    {
	    ParseErrors = parseErrors;
    }
    
    public SvgParserResult(EngineeringSymbol symbol, Dictionary<string, List<string>> parseErrors)
    {
	    EngineeringSymbol = symbol;
	    ParseErrors = parseErrors;
    }
	
}