namespace EngineeringSymbols.Tools.Models;

public record EngineeringSymbolSvgParsed
{
	public required string Geometry { get; init; }
	public required int Width { get; init; }
	public required int Height { get; init; }
}