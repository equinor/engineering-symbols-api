using EngineeringSymbols.Tools.Entities;
using EngineeringSymbols.Tools.Models;
using Newtonsoft.Json.Linq;

namespace EngineeringSymbols.Api.Services;

public interface IEngineeringSymbolService
{
	TryAsync<JObject> GetSymbolsAsync(bool onlyLatestVersion, bool publicVersion);
	TryAsync<JObject> GetSymbolByIdOrIdentifierAsync(string idOrKey, bool publicVersion);
	TryAsync<EngineeringSymbol> CreateSymbolAsync(EngineeringSymbolCreateDto createDto, bool validationOnly);
	//TryAsync<bool> UpdateSymbolAsync(string id, EngineeringSymbolCreateDto createDto);
	//TryAsync<bool> UpdateSymbolStatusAsync(string id, EngineeringSymbolStatusDto statusDto);
	TryAsync<bool> DeleteSymbolAsync(string id);
}