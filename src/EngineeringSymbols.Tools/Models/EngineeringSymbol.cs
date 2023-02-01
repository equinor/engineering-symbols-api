using System.Text.Json;
using System.Text.Json.Serialization;

namespace EngineeringSymbols.Tools.Models;

public record EngineeringSymbol
{
	public string Id { get; init; }
	public string? SvgString { get; init; }

	public string? SvgStringRaw { get; init; }

	public string? GeometryString { get; init; }
	public double Width { get; init; }

	public double Height { get; init; }
	public List<EngineeringSymbolConnector> Connectors { get; init; } = new List<EngineeringSymbolConnector>();

	public override string ToString()
	{
		return SvgString ?? "";
	}

	public string ToJson(bool indent = false)
	{
		JsonSerializerOptions options = new(JsonSerializerDefaults.Web)
		{
			WriteIndented = indent,
			DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
		};

		return JsonSerializer.Serialize(this, options);
	}
}