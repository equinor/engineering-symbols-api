namespace EngineeringSymbols.Tools.SvgParser.Models;

internal class ExtractedSvgData
{
    public int Width { get; set; }
    public int Height { get; set; }
    public List<string> PathData { get; } = new();
}