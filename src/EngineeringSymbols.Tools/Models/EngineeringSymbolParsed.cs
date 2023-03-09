namespace EngineeringSymbols.Tools.Models;

public record EngineeringSymbolParsed
{
	public required string Key { get; init; }
	
	public required string? Description { get; init; }
	public required string? Filename { get; init; }
	public required string SvgString { get; init; }
	public required string GeometryString { get; init; }
	public required double Width { get; init; }
	public required double Height { get; init; }
	public List<EngineeringSymbolConnectorParsed> Connectors { get; init; } = new();
}