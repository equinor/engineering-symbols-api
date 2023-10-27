using System.Text.Json.Serialization;
using EngineeringSymbols.Tools.Entities;
using Newtonsoft.Json.Linq;

namespace EngineeringSymbols.Tools.Models;

public record EngineeringSymbolPutDto
{
    /// <summary>
    /// Identifier for the symbol that is stable over different versions of the symbol.
    /// </summary>
    public string Identifier { get; init; }
    public string UserIdentifier { get; init; }
    public string Label { get; init; }
    public string Description { get; init; }
    public List<User>? Creators { get; init; }
    public List<User>? Contributors { get; init; }
    public Shape Shape { get; init; }
    public int Width { get; init; }
    public int Height { get; init; }
    public Point CenterOfRotation { get; init; }
    public List<ConnectionPoint> ConnectionPoints { get; init; }
    public string? DrawColor { get; init; }
    public string? FillColor { get; init; }
    public List<string>? Sources { get; init; }
    public List<string>? Subjects { get; init; }
}

public record EngineeringSymbolStatusDto(string Status);
