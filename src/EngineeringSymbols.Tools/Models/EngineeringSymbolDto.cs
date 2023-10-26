using System.Text.Json.Serialization;
using EngineeringSymbols.Tools.Entities;
using Newtonsoft.Json.Linq;

namespace EngineeringSymbols.Tools.Models;

[JsonDerivedType(typeof(List<EngineeringSymbol>))]
[JsonDerivedType(typeof(JObject))]
public record EngineeringSymbolResponse;

public static class EngineeringSymbolResponseContentType
{
    public const string Json = "application/json";
    public const string JsonLd = "application/ld+json";
}

public record EngineeringSymbolPutDto
{
    /// <summary>
    /// Identifier for the symbol that is stable over different versions of the symbol.
    /// </summary>
    public required string Identifier { get; init; }
    
    /// <summary>
    /// IRI
    /// </summary>
    //public string? IsRevisionOf { get; init; }
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
    
    public required string UserIdentifier { get; init; }
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
