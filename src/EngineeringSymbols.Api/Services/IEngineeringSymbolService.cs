using EngineeringSymbols.Api.Entities;
using EngineeringSymbols.Api.Models;
using EngineeringSymbols.Tools.Models;

namespace EngineeringSymbols.Api.Services;

public interface IEngineeringSymbolService
{
	TryAsync<IEnumerable<EngineeringSymbolListItemResponseDto>> GetSymbolsAsync();
	TryAsync<EngineeringSymbol> GetSymbolByIdOrKeyAsync(string idOrKey);
	TryAsync<string> CreateSymbolAsync(EngineeringSymbolCreateDto createDto);
	TryAsync<bool> UpdateSymbolAsync(string id, EngineeringSymbolUpdateDto updateDto);
	TryAsync<bool> DeleteSymbolAsync(string id);
}