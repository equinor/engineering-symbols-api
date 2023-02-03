using EngineeringSymbols.Tools.Models;
namespace EngineeringSymbols.Api.Models;

public class EngineeringSymbolConnectorDto
{
	public string Id { get; set; }

	/// <summary>
	/// Position relative to top-left corner.
	/// </summary>
	public Point RelativePosition { get; set; } = new();
	public int Direction { get; set; }
}
