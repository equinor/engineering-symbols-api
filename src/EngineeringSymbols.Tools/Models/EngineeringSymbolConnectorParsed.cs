namespace EngineeringSymbols.Tools.Models;


public record EngineeringSymbolConnectorParsed
{
	public required string Id { get; init; }

	/// <summary>
	/// Position relative to top-left corner.
	/// </summary>
	public required Point RelativePosition { get; init; }

	/// <summary>
	/// Direction in degrees 0-360 where 0 degrees is vertical up (12 o' clock).
	/// </summary>
	public required int Direction { get; init; }
}