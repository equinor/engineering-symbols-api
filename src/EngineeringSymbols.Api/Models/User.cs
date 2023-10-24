namespace EngineeringSymbols.Api.Models;

public record User(Guid ObjectIdentifier, List<string> Roles, string FriendlyName, string Email);