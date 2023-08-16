using System.Security.Claims;
using EngineeringSymbols.Api.Services.EngineeringSymbolService;
using EngineeringSymbols.Tools.Models;

namespace EngineeringSymbols.Api.Services;

public interface IEngineeringSymbolService
{
	TryAsync<IEnumerable<IEngineeringSymbolResponse>> GetSymbolsAsync(bool allVersions = false, bool publicVersion = true);
	TryAsync<IEngineeringSymbolResponse> GetSymbolByIdOrKeyAsync(string idOrKey, bool publicVersion = true);
	TryAsync<string> CreateSymbolAsync(ClaimsPrincipal user, CreateSymbol.InsertContentType contentType, string content,
		bool validationOnly);
	TryAsync<bool> ReplaceSymbolAsync(EngineeringSymbolCreateDto createDto);
	TryAsync<bool> DeleteSymbolAsync(string id);
}