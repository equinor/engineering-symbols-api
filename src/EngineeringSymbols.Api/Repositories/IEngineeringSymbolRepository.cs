using EngineeringSymbols.Api.Entities;

namespace EngineeringSymbols.Api.Repositories;

public interface IEngineeringSymbolRepository
{
    TryAsync<IEnumerable<string>> GetAllEngineeringSymbolsAsync();
    TryAsync<EngineeringSymbol> GetEngineeringSymbolByIdAsync(string id);
    TryAsync<EngineeringSymbol> InsertEngineeringSymbolAsync(EngineeringSymbolCreateDto createDto);
}