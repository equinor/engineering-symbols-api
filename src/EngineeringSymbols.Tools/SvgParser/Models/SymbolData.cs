using EngineeringSymbols.Tools.Models;

namespace EngineeringSymbols.Tools.SvgParser;

internal class SymbolData
{
	public string Id { get; set; }
	public double Width { get; set; }
	public double Height { get; set; }

	public List<string> PathData { get; set; } = new();
	public List<EngineeringSymbolConnector> Connectors { get; set; } = new();
}