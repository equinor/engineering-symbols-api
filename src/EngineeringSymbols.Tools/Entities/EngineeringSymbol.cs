using System.Text.Json.Serialization;

namespace EngineeringSymbols.Tools.Entities;

public record EngineeringSymbol
{
    [JsonIgnore]
    public bool ShouldSerializeAsPublicVersion { get; init; } = true;
    
    /// <summary>
    /// Symbol IRI
    /// </summary>
    public required string Id { get; init; }
    public required EngineeringSymbolStatus Status { get; init; }
    public required string Identifier { get; init; }
    /// <summary>
    /// Version is an integer > 0, but stored as a string for future changes
    /// </summary>
    public required string Version { get; init; }
    public required string? PreviousVersion { get; init; }
    public required string Label { get; init; }
    public required string Description { get; init; }
    public required List<string> Sources { get; init; }
    public required List<string> Subjects { get; init; }
    public required DateTimeOffset DateTimeCreated { get; init; }
    public required DateTimeOffset DateTimeModified { get; init; }
    public required DateTimeOffset DateTimeIssued { get; init; }
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
