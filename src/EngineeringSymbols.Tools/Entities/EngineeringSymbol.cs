namespace EngineeringSymbols.Tools.Entities;

public record EngineeringSymbol
{
    public required string Id { get; init; }
    public required string Key { get; init; }
    public required string Description { get; init; }
    public required DateTimeOffset DateTimeCreated { get; init; }
    public required DateTimeOffset DateTimeUpdated { get; init; }
    public required string Owner { get; init; }
    public required string Filename { get; init; }
    public required string GeometryPath { get; init; }
    public required double Width { get; init; }
    public required double Height { get; init; }
    public List<EngineeringSymbolConnector> Connectors { get; init; } = new();
}
