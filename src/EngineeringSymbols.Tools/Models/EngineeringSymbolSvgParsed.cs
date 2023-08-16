namespace EngineeringSymbols.Tools.Models;

public record EngineeringSymbolSvgParsed
{
	public required string Key { get; init; }
	public required string Geometry { get; init; }
	public required double Width { get; init; }
	public required double Height { get; init; }
}