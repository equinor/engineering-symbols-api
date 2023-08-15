using System.Security.Claims;
using EngineeringSymbols.Tools.Models;

namespace EngineeringSymbols.Api.Services;

public interface IEngineeringSymbolService
{
	TryAsync<IEnumerable<EngineeringSymbolPublicDto>> GetSymbolsPublicAsync(bool allVersions = false);
	TryAsync<IEnumerable<EngineeringSymbolDto>> GetSymbolsAsync(bool allVersions = false);
	TryAsync<EngineeringSymbolPublicDto> GetSymbolByIdOrKeyPublicAsync(string idOrKey);
	TryAsync<EngineeringSymbolDto> GetSymbolByIdOrKeyAsync(string idOrKey);
	TryAsync<string> CreateSymbolFromFileAsync(ClaimsPrincipal user, IFormFile svgFile);
	TryAsync<string> CreateSymbolFromJsonAsync(ClaimsPrincipal user, EngineeringSymbolCreateDto createDto);
	TryAsync<bool> ReplaceSymbolAsync(EngineeringSymbolPutDto putDto);
	TryAsync<bool> DeleteSymbolAsync(string id);
}