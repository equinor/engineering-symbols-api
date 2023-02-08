using EngineeringSymbols.Api.Entities;
using EngineeringSymbols.Api.Models;
using EngineeringSymbols.Api.Repositories;
using EngineeringSymbols.Tools.Models;

namespace EngineeringSymbols.Api.Services;

public class EngineeringSymbolService : IEngineeringSymbolService
{
    private IEngineeringSymbolRepository _Repo;
    
    public EngineeringSymbolService(IConfiguration config, IEngineeringSymbolRepository repo)
    {
        _Repo = repo;
    }

    public TryAsync<IEnumerable<string>> GetSymbolsAsync()
    {
        return _Repo.GetAllEngineeringSymbolsAsync();
    }

    public TryAsync<EngineeringSymbol> GetSymbolAsync(string id)
    {
        return _Repo.GetEngineeringSymbolByIdAsync(id);
    }

    public TryAsync<EngineeringSymbol> CreateSymbolAsync(EngineeringSymbolCreateDto createDto)
    {
        Console.WriteLine($"Save symbol {createDto.Name ?? "?"}!");
        return _Repo.InsertEngineeringSymbolAsync(createDto);
    }
}