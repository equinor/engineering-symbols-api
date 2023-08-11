using EngineeringSymbols.Tools.Entities;

namespace EngineeringSymbols.Api.Repositories;

public interface IEngineeringSymbolRepository
{
    TryAsync<IEnumerable<IEngineeringSymbolResponseDto>> GetAllEngineeringSymbolsAsync(bool distinct = true);
    //TryAsync<IEnumerable<IEngineeringSymbolResponseDto>> GetAllEngineeringSymbolsIncludeAllVersionsAsync();
    TryAsync<EngineeringSymbol> GetEngineeringSymbolByIdAsync(string id);
    TryAsync<EngineeringSymbol> GetEngineeringSymbolByKeyAsync(string key); 
    TryAsync<string> InsertEngineeringSymbolAsync(EngineeringSymbolCreateDto createDto);
    
    TryAsync<bool> UpdateEngineeringSymbolAsync(string id, EngineeringSymbolUpdateDto updateDto);
    
    TryAsync<bool> DeleteEngineeringSymbolAsync(string id);
}