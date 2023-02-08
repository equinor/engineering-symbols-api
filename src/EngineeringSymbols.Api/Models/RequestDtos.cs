using EngineeringSymbols.Api.Entities;

namespace EngineeringSymbols.Api.Models;

/// <summary>
/// Information needed to create a symbol in a Repository
/// </summary>
public record EngineeringSymbolCreateDto
{
    public string Name { get; init; }
    public string Owner { get; init; }
    public string Filename { get; init; }
    public string SvgString { get; init; }
    public string GeometryString { get; init; }
    public double Width { get; init; }
    public double Height { get; init; }
    public List<EngineeringSymbolConnector> Connectors { get; init; } = new();
}

