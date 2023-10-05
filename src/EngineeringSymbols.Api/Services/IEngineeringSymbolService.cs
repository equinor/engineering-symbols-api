using EngineeringSymbols.Tools.Entities;
using EngineeringSymbols.Tools.Models;

namespace EngineeringSymbols.Api.Services;

public interface IEngineeringSymbolService
{
	TryAsync<List<EngineeringSymbol>> GetSymbolsAsync(bool onlyLatestVersion, bool publicVersion);
	TryAsync<List<EngineeringSymbol>> GetSymbolByIdOrIdentifierAsync(string idOrKey, bool publicVersion);
	TryAsync<EngineeringSymbol> CreateSymbolAsync(EngineeringSymbolCreateDto createDto, bool validationOnly);
	//TryAsync<bool> UpdateSymbolAsync(string id, EngineeringSymbolCreateDto createDto);
	//TryAsync<bool> UpdateSymbolStatusAsync(string id, EngineeringSymbolStatusDto statusDto);
	TryAsync<bool> DeleteSymbolAsync(string id);
}