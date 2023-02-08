using System.Xml.Linq;

namespace EngineeringSymbols.Tools.SvgParser.Models;

public enum SvgParseCategory
{
    Dimensions,
    Connector,
    
}

internal class SvgParserContext
{
    public SvgParserOptions Options { get; set; }
    
    public bool HasParseErrors => ParseErrors.Count == 0;
    private Dictionary<string, List<string>> ParseErrors { get; } = new();

    public ExtractedSvgData ExtractedData { get; } = new();
	
    public XElement SvgRootElement { get; set; }
	
    public SvgParserResult ToSvgParserResult()
    {
        return new SvgParserResult(
            new EngineeringSymbolParsed
            {
                Filename = ExtractedData.Filename,
                SvgString = Options.IncludeSvgString ? SvgRootElement.ToString() : null,
                GeometryString = string.Join("", ExtractedData.PathData),
                Width = ExtractedData.Width,
                Height = ExtractedData.Height,
                Connectors = ExtractedData.Connectors,
            }, ParseErrors);
    }

    public void AddParseError(SvgParseCategory category, string errorMessage)
    {
        var symId = ExtractedData.Filename != null ? " - " + ExtractedData.Filename : string.Empty;
        var key = Enum.GetName(typeof(SvgParseCategory), category) + symId;

        if (ParseErrors.ContainsKey(key))
        {
            ParseErrors[key].Add(errorMessage);
        }
        else
        {
            ParseErrors.Add(key, new List<string> {errorMessage});
        }
    }
}
