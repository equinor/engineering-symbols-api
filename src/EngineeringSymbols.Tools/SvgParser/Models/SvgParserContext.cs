using System.Xml.Linq;

namespace EngineeringSymbols.Tools.SvgParser.Models;

internal class SvgParserContext
{
    public SvgParserOptions Options { get; set; }
    
    public bool HasParseErrors => ParseErrors.Count == 0;
    public List<string> ParseErrors { get; } = new();

    public ExtractedSvgData ExtractedData { get; } = new();
	
    public XElement SvgRootElement { get; set; }
	
    public SvgParserResult ToSvgParserResult()
    {
        return new SvgParserResult(
            new EngineeringSymbol
            {
                Id = ExtractedData.Id,
                SvgString = Options.IncludeSvgString ? SvgRootElement.ToString() : null,
                SvgStringRaw = ExtractedData.RawSvgInputString,
                GeometryString = string.Join("", ExtractedData.PathData),
                Width = ExtractedData.Width,
                Height = ExtractedData.Height,
                Connectors = ExtractedData.Connectors,
            }, ParseErrors);
    }

    public void AddParseError(string errorMessage)
    {
        ParseErrors.Add(errorMessage);
    }
	
}