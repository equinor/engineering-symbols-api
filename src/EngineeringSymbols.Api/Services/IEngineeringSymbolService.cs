using EngineeringSymbols.Api.Models;

namespace EngineeringSymbols.Api.Services;

public interface IEngineeringSymbolService
{
	//Task<List<string>> GetSymbols();

	//Task<List<EngineeringSymbolDto>> GetSymbolsDetailed();

	//Task<EngineeringSymbolDto> GetSymbol(string id);
	
	Task<EngineeringSymbolDto> SaveSymbol(EngineeringSymbolDto dto);
}