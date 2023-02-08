namespace EngineeringSymbols.Tools.Models;

public record EngineeringSymbolParsed
{
	public string? Filename { get; init; }
	public string SvgString { get; init; }
	public string GeometryString { get; init; }
	public double Width { get; init; }
	public double Height { get; init; }
	public List<EngineeringSymbolConnectorParsed> Connectors { get; init; } = new();
}