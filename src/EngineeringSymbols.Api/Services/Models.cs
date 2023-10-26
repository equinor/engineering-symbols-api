using EngineeringSymbols.Tools.Models;

namespace EngineeringSymbols.Api.Services;

public record SymbolSlim(string Id, string Identifier, DateTime DateIssued, string Version, string? IsRevisionOf);

public record SymbolStatusInfo(string Id, string Identifier, EngineeringSymbolStatus Status, string? Version, string? PreviousVersion);