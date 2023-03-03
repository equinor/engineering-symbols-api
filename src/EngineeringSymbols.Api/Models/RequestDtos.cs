using EngineeringSymbols.Tools.Entities;

namespace EngineeringSymbols.Api.Models;

/// <summary>
/// Information needed to create a symbol in a Repository
/// </summary>
public record EngineeringSymbolCreateDto
{
    public required string Key { get; init; }
    public required string Owner { get; init; }
    public required string Filename { get; init; }
    public required string SvgString { get; init; }
    public required string GeometryString { get; init; }
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


public static class Test
{
    public static void Test2()
    {


    }
}
