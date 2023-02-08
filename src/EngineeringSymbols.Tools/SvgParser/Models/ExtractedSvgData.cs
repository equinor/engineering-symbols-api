namespace EngineeringSymbols.Tools.SvgParser.Models;

internal class ExtractedSvgData
{
    public string? Filename { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
    public List<string> PathData { get; } = new();
    public string? RawSvgInputString { get; set; }
    public List<EngineeringSymbolConnectorParsed> Connectors { get; } = new();
}