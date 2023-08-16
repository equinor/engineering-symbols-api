using System.Security.Claims;
using EngineeringSymbols.Api.Services.EngineeringSymbolService;
using EngineeringSymbols.Tools.Models;

namespace EngineeringSymbols.Api.Services;

public interface IEngineeringSymbolService
{
	TryAsync<IEnumerable<EngineeringSymbolPublicDto>> GetSymbolsPublicAsync(bool allVersions = false);
	TryAsync<IEnumerable<EngineeringSymbolDto>> GetSymbolsAsync(bool allVersions = false);
	TryAsync<EngineeringSymbolPublicDto> GetSymbolByIdOrKeyPublicAsync(string idOrKey);
	TryAsync<EngineeringSymbolDto> GetSymbolByIdOrKeyAsync(string idOrKey);

	TryAsync<string> CreateSymbolAsync(ClaimsPrincipal user, CreateSymbol.InsertContentType contentType, string content,
		bool validationOnly);
	TryAsync<bool> ReplaceSymbolAsync(EngineeringSymbolCreateDto createDto);
	TryAsync<bool> DeleteSymbolAsync(string id);
}