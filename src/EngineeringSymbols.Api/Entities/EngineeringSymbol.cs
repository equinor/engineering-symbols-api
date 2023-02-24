using EngineeringSymbols.Tools.Models;

namespace EngineeringSymbols.Api.Entities;

public record EngineeringSymbol
{
    public required string Id { get; init; }
    public required string Key { get; init; }
    public required DateTimeOffset DateTimeCreated { get; init; }
    public required DateTimeOffset DateTimeUpdated { get; init; }
    public required string Owner { get; init; }
    public required string Filename { get; init; }
    public required string SvgString { get; init; }
    public required string GeometryString { get; init; }
    public required double Width { get; init; }
    public required double Height { get; init; }
    public List<EngineeringSymbolConnector> Connectors { get; init; } = new();
    



}