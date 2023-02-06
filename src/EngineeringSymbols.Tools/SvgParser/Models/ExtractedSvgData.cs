namespace EngineeringSymbols.Tools.SvgParser.Models;

internal class ExtractedSvgData
{
    public string Id { get; set; }
	
    public string Filename { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
    public List<string> PathData { get; } = new();
    public string? RawSvgInputString { get; set; }
    public List<EngineeringSymbolConnector> Connectors { get; } = new();
}