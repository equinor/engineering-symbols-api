using EngineeringSymbols.Tools.Models;

namespace EngineeringSymbols.Api.Services;

public interface IEngineeringSymbolService
{
	TryAsync<IEnumerable<EngineeringSymbolResponse>> GetSymbolsAsync(bool onlyLatestVersion, bool publicVersion);
	TryAsync<IEnumerable<EngineeringSymbolResponse>> GetSymbolByIdOrKeyAsync(string idOrKey, bool publicVersion);
	TryAsync<EngineeringSymbolDto> CreateSymbolAsync(EngineeringSymbolCreateDto createDto, bool validationOnly);
	TryAsync<bool> UpdateSymbolAsync(string id, EngineeringSymbolCreateDto createDto);
	TryAsync<bool> UpdateSymbolStatusAsync(string id, EngineeringSymbolStatusDto statusDto);
	TryAsync<bool> DeleteSymbolAsync(string id);
}