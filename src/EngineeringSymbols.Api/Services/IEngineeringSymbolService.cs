using EngineeringSymbols.Tools.Entities;
using EngineeringSymbols.Tools.Models;
using Newtonsoft.Json.Linq;

namespace EngineeringSymbols.Api.Services;

public interface IEngineeringSymbolService
{
	TryAsync<JObject> GetSymbolsAsync(bool onlyLatestVersion, bool publicVersion);
	TryAsync<JObject> GetSymbolByIdOrIdentifierAsync(string idOrKey, bool publicVersion);
	TryAsync<EngineeringSymbol> CreateSymbolAsync(EngineeringSymbolPutDto putDto, bool validationOnly);
	TryAsync<EngineeringSymbol> UpdateSymbolAsync(string id, EngineeringSymbolPutDto putDto);
	TryAsync<Unit> UpdateSymbolStatusAsync(string id, EngineeringSymbolStatusDto statusDto);
	TryAsync<Unit> DeleteSymbolAsync(string id);
}