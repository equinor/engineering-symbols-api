using EngineeringSymbols.Tools.Entities;

namespace EngineeringSymbols.Tools.Models;

public record EngineeringSymbolPublicDto
{
    public required string Id { get; init; }
    public required string Key { get; init; }
    public required string Description { get; init; }
    public required DateTimeOffset DateTimePublished { get; init; }
    public required string Geometry { get; init; }
    public required double Width { get; init; }
    public required double Height { get; init; }
    public required List<EngineeringSymbolConnectorPublicDto> Connectors { get; set; } = new();
}

public record EngineeringSymbolDto
{
    public required string? Id { get; init; }
    public required string? Key { get; init; }
    public required string? Status { get; init; }
    public required string? Description { get; init; }
    public required DateTimeOffset? DateTimeCreated { get; init; }
    public required DateTimeOffset? DateTimeUpdated { get; init; }
    
    public required DateTimeOffset? DateTimePublished { get; init; }
    public required string? Owner { get; init; }
    public required string? Filename { get; init; }
    public required string? Geometry { get; init; }
    public required double? Width { get; init; }
    public required double? Height { get; init; }
    public required List<EngineeringSymbolConnector>? Connectors { get; set; } = new();
}

public record EngineeringSymbolConnectorDto 
{
    public required string? Id { get; init; }
    public required Point? RelativePosition { get; init; }
    public required int? Direction { get; init; }
}

public record EngineeringSymbolConnectorPublicDto : EngineeringSymbolConnectorDto;

public record EngineeringSymbolCreateDto
{
    public required string? Key { get; init; }
    public required string? Description { get; init; }
    public required string? Owner { get; init; }
    public required string? Filename { get; init; }
    public required string? Geometry { get; init; }
    public required double? Width { get; init; }
    public required double? Height { get; init; }
    public required List<EngineeringSymbolConnector>? Connectors { get; init; } = new();
}