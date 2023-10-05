using System.Text.Json.Serialization;
using EngineeringSymbols.Tools.Entities;

namespace EngineeringSymbols.Tools.Models;

// [JsonDerivedType(typeof(EngineeringSymbolDto))]
// [JsonDerivedType(typeof(EngineeringSymbolPublicDto))]
// public record EngineeringSymbolResponse;


public record EngineeringSymbolCreateDto
{
    /// <summary>
    /// Identifier for the symbol that is stable over different versions of the symbol.
    /// </summary>
    public required string Identifier { get; init; }
    
    /// <summary>
    /// IRI
    /// </summary>
    public string? IsRevisionOf { get; init; }
    public required string Label { get; init; }
    public required string Description { get; init; }
    /// <summary>
    /// Reference to the source of the symbol, if the symbol is taken from a diagram standard that can be referenced.
    /// </summary>
    public required List<string>? Sources { get; init; }
    
    /// <summary>
    /// Reference to the origin of the symbol, if the origin symbol can be referenced.
    /// </summary>
    public required List<string>? Subjects { get; init; }
    public required List<User> Creators { get; init; }
    public required List<User> Contributors { get; init; }
    public required Shape Shape { get; init; }
    public required int Width { get; init; }
    public required int Height { get; init; }
    public required string? DrawColor { get; init; }
    public required string? FillColor { get; init; }
    public required Point CenterOfRotation { get; init; }
    public required List<ConnectionPoint> ConnectionPoints { get; init; }
}

public record EngineeringSymbolStatusDto(string Status);
