namespace EngineeringSymbols.Tools.SvgParser.Models;

public record SvgParserResult
{
    public EngineeringSymbolParsed? EngineeringSymbolParsed { get; }
    
    public bool IsSuccess => ParseErrors.Count == 0 && EngineeringSymbolParsed != null;
    public Dictionary<string, List<string>> ParseErrors { get; } = new();

    public SvgParserResult(EngineeringSymbolParsed symbolParsed)
    {
	    EngineeringSymbolParsed = symbolParsed;
    }
    
    public SvgParserResult(Dictionary<string, List<string>> parseErrors)
    {
	    ParseErrors = parseErrors;
    }
    
    public SvgParserResult(EngineeringSymbolParsed symbolParsed, Dictionary<string, List<string>> parseErrors)
    {
	    EngineeringSymbolParsed = symbolParsed;
	    ParseErrors = parseErrors;
    }
	
}