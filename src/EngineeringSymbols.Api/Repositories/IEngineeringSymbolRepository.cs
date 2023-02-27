using EngineeringSymbols.Tools.Entities;

namespace EngineeringSymbols.Api.Repositories;

public interface IEngineeringSymbolRepository
{
    TryAsync<IEnumerable<EngineeringSymbolListLatestItemResponseDto>> GetAllEngineeringSymbolsAsync();
    TryAsync<IEnumerable<EngineeringSymbolListItemResponseDto>> GetAllEngineeringSymbolsIncludeAllVersionsAsync();
    TryAsync<EngineeringSymbol> GetEngineeringSymbolAsync(string idOrKey);
    TryAsync<string> InsertEngineeringSymbolAsync(EngineeringSymbolCreateDto createDto);
    
    TryAsync<bool> UpdateEngineeringSymbolAsync(string id, EngineeringSymbolUpdateDto updateDto);
    
    TryAsync<bool> DeleteEngineeringSymbolAsync(string id);
}