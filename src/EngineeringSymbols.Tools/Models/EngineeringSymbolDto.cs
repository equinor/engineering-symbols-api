using EngineeringSymbols.Tools.Entities;

namespace EngineeringSymbols.Tools.Models;

public record EngineeringSymbolPublicDto(
    string Id,
    string Key,
    string Description,
    DateTimeOffset DateTimePublished,
    string Geometry,
    double Width,
    double Height,
    List<EngineeringSymbolConnectorPublicDto> Connectors
);

public record EngineeringSymbolConnectorPublicDto(string Id, Point RelativePosition, int Direction);

public record EngineeringSymbolDto(
    string Id,
    string Key,
    string Status,
    string Description,
    DateTimeOffset DateTimeCreated,
    DateTimeOffset DateTimeUpdated,
    DateTimeOffset DateTimePublished,
    string Owner,
    string Geometry,
    double Width,
    double Height,
    List<EngineeringSymbolConnectorDto> Connectors
);

public record EngineeringSymbolConnectorDto(string Id, Point RelativePosition, int Direction);

public record EngineeringSymbolCreateDto(
    string Key,
    string Description,
    string Owner,
    string Geometry,
    double Width,
    double Height,
    List<EngineeringSymbolConnectorDto> Connectors);
    