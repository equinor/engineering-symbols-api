using EngineeringSymbols.Tools.Entities;

namespace EngineeringSymbols.Api.Models;

public interface IEngineeringSymbolResponseDto {}

public record EngineeringSymbolResponseDto : IEngineeringSymbolResponseDto
{
    public required string Id { get; init; }
    public required string Key { get; init; }
    public required string Description { get; init; }
    public required DateTimeOffset DateTimeCreated { get; init; }
    public required DateTimeOffset DateTimeUpdated { get; init; }
    public required string Geometry { get; init; }
    public required double Width { get; init; }
    public required double Height { get; init; }
    public List<EngineeringSymbolConnector> Connectors { get; init; } = new();
}

/// <summary>
/// Full representation as in DB
/// </summary>
public class EngineeringSymbolCompleteResponseDto : IEngineeringSymbolResponseDto //: EngineeringSymbol
{
    public string? Id { get; set; }
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

public class EngineeringSymbolListItemResponseDto : IEngineeringSymbolResponseDto
{
    public required string Id { get; set; }
    public required string Key { get; set; }
}

public class EngineeringSymbolListLatestItemResponseDto : IEngineeringSymbolResponseDto
{
    public required string Key { get; set; }
    public required string IdLatestVersion { get; set; }
    public required int NumberOfVersions { get; set; }
}

public class EngineeringSymbolListLatestItemResponseDto2 : IEngineeringSymbolResponseDto
{
    public required string Key { get; set; }
    public required int NumberOfVersions { get; set; }
    public required EngineeringSymbolResponseDto LatestVersion { get; set; }
}

