using EngineeringSymbols.Tools.Models;

namespace EngineeringSymbols.Api.Entities;

public record EngineeringSymbol : EngineeringSymbolParsed
{
    public string Id { get; init; }
    public string Name { get; init; }
    public DateTimeOffset DateTimeCreated { get; init; }
    public DateTimeOffset DateTimeUpdated { get; init; }
    public string Owner { get; init; }
    public List<EngineeringSymbolConnector> Connectors { get; init; } = new();
}