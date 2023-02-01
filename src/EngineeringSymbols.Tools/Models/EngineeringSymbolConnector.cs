namespace EngineeringSymbols.Tools.Models;

public record EngineeringSymbolConnector
{
	public string Id { get; set; }
	public Point RelativePosition { get; set; } = new Point() { X = 0, Y = 0 };
	public int Direction { get; set; }
}