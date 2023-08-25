namespace EngineeringSymbols.Tools.SvgParser.Models;

internal class ExtractedSvgData
{
    public string? Key { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public List<string> PathData { get; } = new();
}