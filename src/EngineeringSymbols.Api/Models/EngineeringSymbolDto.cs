using System.Text.Json;
using System.Text.Json.Serialization;

namespace EngineeringSymbols.Api.Models;

public record EngineeringSymbolDto
{
	public string Id { get; init; }

	public string? SvgString { get; init; }

	public string? SvgStringRaw { get; init; }

	public string? GeometryString { get; init; }
	public double Width { get; init; }

	public double Height { get; init; }

	public List<EngineeringSymbolConnectorDto> Connectors { get; init; } = new();
}