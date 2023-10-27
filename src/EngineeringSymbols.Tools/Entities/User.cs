namespace EngineeringSymbols.Tools.Entities;

/// <summary>
/// 
/// </summary>
/// <param name="Name"></param>
/// <param name="Email"></param>
public record User
{
    public string Name { get; init; }
    public string Email { get; init; }
}