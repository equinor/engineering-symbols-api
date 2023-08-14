namespace EngineeringSymbols.Tools.Entities;

public enum EngineeringSymbolStatus
{
    None,
    Draft,
    Review,
    Accepted,
    Rejected,
}

public record EngineeringSymbol(
    string Id,
    string Key,
    EngineeringSymbolStatus Status,
    string Description,
    DateTimeOffset DateTimeCreated,
    DateTimeOffset DateTimeUpdated,
    string Owner,
    string Filename,
    string Geometry,
    double Width,
    double Height,
    List<EngineeringSymbolConnector> Connectors
);
