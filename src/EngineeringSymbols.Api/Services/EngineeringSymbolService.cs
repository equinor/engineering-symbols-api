using EngineeringSymbols.Api.Repositories;
using EngineeringSymbols.Tools.Entities;
using EngineeringSymbols.Tools.Validation;

namespace EngineeringSymbols.Api.Services;

public class EngineeringSymbolService : IEngineeringSymbolService
{
    private IEngineeringSymbolRepository _Repo;
    
    public EngineeringSymbolService(IConfiguration config, IEngineeringSymbolRepository repo)
    {
        _Repo = repo;
    }

    public TryAsync<IEnumerable<EngineeringSymbolListItemResponseDto>> GetSymbolsAsync()
    {
        return _Repo.GetAllEngineeringSymbolsIncludeAllVersionsAsync();
    }

    public TryAsync<IEnumerable<EngineeringSymbolListLatestItemResponseDto>> GetSymbolsLatestAsync()
    {
            return _Repo.GetAllEngineeringSymbolsAsync();
    }

    public TryAsync<EngineeringSymbol> GetSymbolByIdOrKeyAsync(string idOrKey)
    {
        return _Repo.GetEngineeringSymbolAsync(idOrKey);
    }
    
    public TryAsync<string> CreateSymbolAsync(EngineeringSymbolCreateDto createDto)
    {
        Console.WriteLine($"Save symbol {createDto.Key ?? "?"}!");
        return _Repo.InsertEngineeringSymbolAsync(createDto);
    }

    public TryAsync<bool> UpdateSymbolAsync(string id, EngineeringSymbolUpdateDto updateDto) =>
        async () =>
        {
            var idParsed = EngineeringSymbolValidation.ParseEngineeringSymbolId(id);
            return await _Repo.UpdateEngineeringSymbolAsync(idParsed.ToString(), updateDto).Try();
        };

    public TryAsync<bool> DeleteSymbolAsync(string id) =>
        async () =>
        {
            var idParsed = EngineeringSymbolValidation.ParseEngineeringSymbolId(id);
            return await _Repo.DeleteEngineeringSymbolAsync(idParsed.ToString()).Try();
        };
}