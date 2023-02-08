using EngineeringSymbols.Api.Entities;
using EngineeringSymbols.Api.Models;
using EngineeringSymbols.Tools.Models;

namespace EngineeringSymbols.Api.Services;

public interface IEngineeringSymbolService
{
	TryAsync<IEnumerable<string>> GetSymbolsAsync();
	
	TryAsync<EngineeringSymbol> GetSymbolAsync(string id);
	
	TryAsync<EngineeringSymbol> CreateSymbolAsync(EngineeringSymbolCreateDto createDto);
}