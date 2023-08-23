namespace EngineeringSymbols.Tools.SvgParser.Models;

public record SvgParserResult
{
    public EngineeringSymbolSvgParsed? EngineeringSymbolSvgParsed { get; }
    
    public bool IsSuccess => ParseErrors.Count == 0 && EngineeringSymbolSvgParsed != null;
    public Dictionary<string, List<string>> ParseErrors { get; } = new();

    public SvgParserResult(EngineeringSymbolSvgParsed symbolParsed, Dictionary<string, List<string>> parseErrors)
    {
	    EngineeringSymbolSvgParsed = symbolParsed;
	    ParseErrors = parseErrors;
    }
}