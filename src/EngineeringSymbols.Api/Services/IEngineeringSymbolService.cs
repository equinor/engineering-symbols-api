using System.Security.Claims;
using EngineeringSymbols.Api.Services.EngineeringSymbolService;
using EngineeringSymbols.Tools.Models;

namespace EngineeringSymbols.Api.Services;

public interface IEngineeringSymbolService
{
	TryAsync<IEnumerable<EngineeringSymbolResponse>> GetSymbolsAsync(bool allVersions = false, bool publicVersion = true);
	TryAsync<IEnumerable<EngineeringSymbolResponse>> GetSymbolByIdOrKeyAsync(string idOrKey, bool publicVersion = true);
	TryAsync<string> CreateSymbolAsync(EngineeringSymbolCreateDto createDto, bool validationOnly);
	TryAsync<bool> UpdateSymbolAsync(string id, EngineeringSymbolCreateDto createDto, bool isSuperAdmin = false);
	TryAsync<bool> UpdateSymbolStatusAsync(string id, EngineeringSymbolStatusDto statusDto);
	TryAsync<bool> DeleteSymbolAsync(string id);
}