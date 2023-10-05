using EngineeringSymbols.Api.Repositories.Fuseki;
using EngineeringSymbols.Tools.Entities;
using EngineeringSymbols.Tools.Models;

namespace EngineeringSymbols.Api.Repositories;

public interface IEngineeringSymbolRepository
{
    TryAsync<List<EngineeringSymbol>> GetAllEngineeringSymbolsAsync(bool onlyLatestVersion, bool onlyPublished);
    TryAsync<List<EngineeringSymbol>> GetEngineeringSymbolByIdAsync(string id, bool onlyPublished);
    TryAsync<List<EngineeringSymbol>> GetEngineeringSymbolByIdentifierAsync(string key, bool onlyPublished); 
    TryAsync<EngineeringSymbol> InsertEngineeringSymbolAsync(EngineeringSymbol symbol);
    //TryAsync<bool> ReplaceEngineeringSymbolAsync(EngineeringSymbol symbol);
    TryAsync<bool> DeleteEngineeringSymbolAsync(string id);
    TryAsync<FusekiRawResponse> FusekiQueryAsync(string query, string accept);
    TryAsync<FusekiRawResponse> FusekiUpdateAsync(string query, string accept);
}