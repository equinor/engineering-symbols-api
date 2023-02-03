namespace EngineeringSymbols.Tools.Models;

public record EngineeringSymbolConnector
{
	public string Id { get; set; }

	/// <summary>
	/// Position relative to top-left corner.
	/// </summary>
	public Point RelativePosition { get; set; } = new();

	/// <summary>
	/// Direction in degrees 0-360 where 0 degrees is vertical up (12 o' clock).
	/// </summary>
	public int Direction { get; set; }
}