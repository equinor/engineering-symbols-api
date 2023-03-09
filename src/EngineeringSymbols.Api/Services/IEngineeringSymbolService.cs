using System.Security.Claims;
using EngineeringSymbols.Api.Models;
using EngineeringSymbols.Tools.Entities;
using EngineeringSymbols.Tools.Models;

namespace EngineeringSymbols.Api.Services;

public interface IEngineeringSymbolService
{
	TryAsync<IEnumerable<IEngineeringSymbolResponseDto>> GetSymbolsAsync(bool? allVersions = false, int? detailLevel = 0);
	TryAsync<EngineeringSymbol> GetSymbolByIdOrKeyAsync(string idOrKey);
	TryAsync<string> CreateSymbolAsync(ClaimsPrincipal user, IFormFile svgFile);
	TryAsync<bool> UpdateSymbolAsync(string id, EngineeringSymbolUpdateDto updateDto);
	TryAsync<bool> DeleteSymbolAsync(string id);
}