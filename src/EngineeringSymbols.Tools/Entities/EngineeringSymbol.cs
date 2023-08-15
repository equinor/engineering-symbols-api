namespace EngineeringSymbols.Tools.Entities;

public record EngineeringSymbol(
    string Id,
    string Key,
    EngineeringSymbolStatus Status,
    string Description,
    DateTimeOffset DateTimeCreated,
    DateTimeOffset DateTimeUpdated,
    DateTimeOffset DateTimePublished,
    string Owner,
    string Filename,
    string Geometry,
    double Width,
    double Height,
    List<EngineeringSymbolConnector> Connectors
);
