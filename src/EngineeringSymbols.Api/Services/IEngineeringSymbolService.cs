using EngineeringSymbols.Api.Models;

namespace EngineeringSymbols.Api.Services;

public interface IEngineeringSymbolService
{
	TryAsync<List<string>> GetSymbolsAsync();

	//Task<List<EngineeringSymbolDto>> GetSymbolsDetailed();

	TryAsync<EngineeringSymbolDto> GetSymbolAsync(string id);
	
	TryAsync<EngineeringSymbolDto> SaveSymbolAsync(EngineeringSymbolDto dto);
}