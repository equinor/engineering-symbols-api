using EngineeringSymbols.Api.Entities;
using EngineeringSymbols.Tools.Models;

namespace EngineeringSymbols.Api.Models;

public record EngineeringSymbolResponseDto
{
    public string Id { get; init; }
    public string Name { get; init; }
    public string SvgString { get; init; }
    public string GeometryString { get; init; }
    public double Width { get; init; }
    public double Height { get; init; }
    public List<EngineeringSymbolConnector> Connectors { get; init; } = new();
}

/// <summary>
/// Full representation as in DB
/// </summary>
public record EngineeringSymbolCompleteResponseDto : EngineeringSymbol { }