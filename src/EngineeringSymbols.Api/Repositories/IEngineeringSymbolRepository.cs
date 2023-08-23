using EngineeringSymbols.Tools.Entities;
using EngineeringSymbols.Tools.Models;

namespace EngineeringSymbols.Api.Repositories;

public interface IEngineeringSymbolRepository
{
    TryAsync<List<EngineeringSymbol>> GetAllEngineeringSymbolsAsync(bool onlyLatestVersion, bool onlyPublished);
    TryAsync<List<EngineeringSymbol>> GetEngineeringSymbolByIdAsync(string id, bool onlyPublished);
    TryAsync<List<EngineeringSymbol>> GetEngineeringSymbolByKeyAsync(string key, bool onlyPublished); 
    TryAsync<string> InsertEngineeringSymbolAsync(EngineeringSymbolCreateDto symbol);
    TryAsync<bool> ReplaceEngineeringSymbolAsync(EngineeringSymbolDto symbol);
    TryAsync<bool> DeleteEngineeringSymbolAsync(string id);
}