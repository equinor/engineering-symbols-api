using EngineeringSymbols.Api.Entities;
using EngineeringSymbols.Tools.Models;

namespace EngineeringSymbols.Api.Models;

public record EngineeringSymbolResponseDto
{
    public required string Id { get; init; }
    public required string Key { get; init; }
    public required string SvgString { get; init; }
    public required string GeometryString { get; init; }
    public required double Width { get; init; }
    public required double Height { get; init; }
    public List<EngineeringSymbolConnector> Connectors { get; init; } = new();
}

/// <summary>
/// Full representation as in DB
/// </summary>
public record EngineeringSymbolCompleteResponseDto : EngineeringSymbol { }


public class EngineeringSymbolListItemResponseDto
{
    public required string Id { get; set; }
    public required string Key { get; set; }
}