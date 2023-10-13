using EngineeringSymbols.Api.Repositories.Fuseki;
using EngineeringSymbols.Tools.Entities;
using EngineeringSymbols.Tools.Models;
using Newtonsoft.Json.Linq;

namespace EngineeringSymbols.Api.Repositories;

public interface IEngineeringSymbolRepository
{
    TryAsync<string> GetAllEngineeringSymbolsAsync(bool onlyLatestVersion, bool onlyPublished);
    TryAsync<string> GetEngineeringSymbolByIdAsync(string id, bool onlyPublished);
    TryAsync<string> GetEngineeringSymbolByIdentifierAsync(string key, bool onlyPublished); 
    TryAsync<EngineeringSymbol> PutEngineeringSymbolAsync(EngineeringSymbol symbol);
    TryAsync<bool> UpdateSymbolStatusAsync(string id, string status);
    TryAsync<bool> DeleteEngineeringSymbolAsync(string id);
    TryAsync<FusekiRawResponse> FusekiQueryAsync(string query, string accept);
    TryAsync<FusekiRawResponse> FusekiUpdateAsync(string query, string accept);
}