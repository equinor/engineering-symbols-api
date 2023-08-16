namespace EngineeringSymbols.Tools.SvgParser.Models;

internal class ExtractedSvgData
{
    public string Key { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
    public List<string> PathData { get; } = new();
}