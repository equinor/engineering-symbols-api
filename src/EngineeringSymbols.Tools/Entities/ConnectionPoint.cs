namespace EngineeringSymbols.Tools.Entities;

/// <summary>
/// Represents an engineering symbol connection point.
/// </summary>
/// <param name="Identifier"></param>
/// <param name="Position"></param>
/// <param name="Direction"></param>
public record ConnectionPoint(string Identifier, Point Position, int Direction);