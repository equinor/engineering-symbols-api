using EngineeringSymbols.Tools.Entities;

namespace EngineeringSymbols.Api.Models;

public record EngineeringSymbolDto
{
    public string Id { get; set; }
    public string Key { get; set; }
    public string Status { get; set; }
    public string Description { get; set; }
    public DateTimeOffset DateTimeCreated { get; set; }
    public DateTimeOffset DateTimeUpdated { get; set; }
    public string Owner { get; set; }
    public string Filename { get; set; }
    public string Geometry { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
    public List<EngineeringSymbolConnector> Connectors { get; set; } = new();
}

/// <summary>
/// Information needed to create a symbol in a Repository
/// </summary>
public record EngineeringSymbolCreateDto
{
    public required string Key { get; init; }
    public required string Description { get; init; }
    public required string Owner { get; init; }
    public required string Filename { get; init; }
    public required string Geometry { get; init; }
    public required double Width { get; init; }
    public required double Height { get; init; }
    public List<EngineeringSymbolConnector> Connectors { get; init; } = new();
}

public record EngineeringSymbolUpdateDto
{
    public string? Key { get; init; }
    public string? Owner { get; init; }
    public string? Description { get; init; }
}