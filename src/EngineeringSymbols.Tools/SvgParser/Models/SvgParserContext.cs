using System.Xml.Linq;

namespace EngineeringSymbols.Tools.SvgParser.Models;

public enum SvgParseCategory
{
    Dimensions,
    Geometry,
}

internal class SvgParserContext
{
    private Dictionary<string, List<string>> ParseErrors { get; } = new();

    public ExtractedSvgData ExtractedData { get; } = new();
	
    public XElement? SvgRootElement { get; set; }
	
    public SvgParserResult ToSvgParserResult()
    {
        if (SvgRootElement == null)
        {
            throw new SvgParseException("SvgRootElement was null");
        }

        return new SvgParserResult(
            new EngineeringSymbolSvgParsed
            {
                Key = ExtractedData.Key,
                Geometry = string.Join("", ExtractedData.PathData),
                Width = ExtractedData.Width,
                Height = ExtractedData.Height,
            }, ParseErrors);
    }

    public void AddParseError(SvgParseCategory category, string errorMessage)
    {
        var key = Enum.GetName(typeof(SvgParseCategory), category);

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
