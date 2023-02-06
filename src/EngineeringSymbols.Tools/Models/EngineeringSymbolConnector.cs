namespace EngineeringSymbols.Tools.Models;


public record EngineeringSymbolConnector2(string Id, Point RelativePosition, int Direction)
{
	public Point RelativePosition { get; init; } = new();
}

public record EngineeringSymbolConnector
{
	public string Id { get; init; }

	/// <summary>
	/// Position relative to top-left corner.
	/// </summary>
	public Point RelativePosition { get; init; } = new();

	/// <summary>
	/// Direction in degrees 0-360 where 0 degrees is vertical up (12 o' clock).
	/// </summary>
	public int Direction { get; init; }
}