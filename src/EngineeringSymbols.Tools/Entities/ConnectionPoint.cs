namespace EngineeringSymbols.Tools.Entities;

/// <summary>
/// Represents an engineering symbol connection point.
/// </summary>
/// <param name="Identifier"></param>
/// <param name="Position"></param>
/// <param name="Direction"></param>
public record ConnectionPoint
{
    public string Identifier { get; init; }
    public Point Position { get; init; }
    public  int Direction { get; init; }
}