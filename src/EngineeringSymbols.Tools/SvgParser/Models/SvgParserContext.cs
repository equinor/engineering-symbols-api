using System.Xml.Linq;

namespace EngineeringSymbols.Tools.SvgParser.Models;

public enum SvgParseCategory
{
    Key,
    Dimensions,
    Connector,
}

internal class SvgParserContext
{
    public required SvgParserOptions Options { get; init; }
    
    private Dictionary<string, List<string>> ParseErrors { get; } = new();

    public ExtractedSvgData ExtractedData { get; } = new();
	
    public XElement? SvgRootElement { get; set; }
	
    public SvgParserResult ToSvgParserResult()
    {
        if (SvgRootElement == null)
        {
            throw new SvgParseException("SvgRootElement was null");
        }
        
        if (ExtractedData.Key == null)
        {
            throw new SvgParseException("Symbol key was null");
        }
        
        return new SvgParserResult(
            new EngineeringSymbolParsed
            {
                Key = ExtractedData.Key,
                Description = ExtractedData.Description,
                Filename = ExtractedData.Filename,
                SvgString = SvgRootElement.ToString(),
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

        if (ParseErrors.TryGetValue(key, out var errorList))
        {
            errorList.Add(errorMessage);
        }
        else
        {
            ParseErrors.Add(key, new List<string> {errorMessage});
        }
    }
}
