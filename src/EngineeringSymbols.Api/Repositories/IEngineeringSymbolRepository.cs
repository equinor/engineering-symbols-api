using EngineeringSymbols.Tools.Entities;
using EngineeringSymbols.Tools.Models;

namespace EngineeringSymbols.Api.Repositories;

public interface IEngineeringSymbolRepository
{
    TryAsync<IEnumerable<EngineeringSymbol>> GetAllEngineeringSymbolsAsync(bool distinct = true, bool onlyPublished = true);

    TryAsync<EngineeringSymbol> GetEngineeringSymbolByIdAsync(string id, bool onlyPublished = true);
    TryAsync<EngineeringSymbol> GetEngineeringSymbolByKeyAsync(string key, bool onlyPublished = true); 
    TryAsync<string> InsertEngineeringSymbolAsync(EngineeringSymbolDto symbol);
    
    TryAsync<bool> ReplaceEngineeringSymbolAsync(EngineeringSymbolDto symbol);
    
    TryAsync<bool> DeleteEngineeringSymbolAsync(string id);
}