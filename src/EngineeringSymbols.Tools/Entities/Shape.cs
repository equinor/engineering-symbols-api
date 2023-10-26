using System.Text.Json.Serialization;

namespace EngineeringSymbols.Tools.Entities;

/// <summary>
/// A shape is a description of a graphical form. A symbol has exactly one shape, but the shape can have multiple serialisations.
/// </summary>
/// <param name="Serializations">A (textual) serialization of the symbol.</param>
/// <param name="Depictions">Depictions or images of the symbol (IRIs)</param>
public record Shape
{
    public List<ShapeSerialization> Serializations { get; init; }
    public List<string> Depictions { get; init; } = new();
}

public record ShapeSerialization
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required ShapeSerializationType Type { get; init; }
    
    public required string Value { get; init; }
}

public enum ShapeSerializationType
{
    SvgPathData
}

