using EngineeringSymbols.Tools.Entities;
using EngineeringSymbols.Tools.Models;

namespace EngineeringSymbols.Api.Repositories;

public interface IEngineeringSymbolRepository
{
    TryAsync<IEnumerable<EngineeringSymbol>> GetAllEngineeringSymbolsAsync(bool distinct = true);

    TryAsync<EngineeringSymbol> GetEngineeringSymbolByIdAsync(string id);
    TryAsync<EngineeringSymbol> GetEngineeringSymbolByKeyAsync(string key); 
    TryAsync<string> InsertEngineeringSymbolAsync(EngineeringSymbolDto symbol);
    
    TryAsync<bool> ReplaceEngineeringSymbolAsync(EngineeringSymbolDto symbol);
    
    TryAsync<bool> DeleteEngineeringSymbolAsync(string id);
}