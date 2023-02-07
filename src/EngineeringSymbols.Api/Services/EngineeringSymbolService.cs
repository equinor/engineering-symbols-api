using EngineeringSymbols.Api.Models;
using EngineeringSymbols.Api.Repositories;

namespace EngineeringSymbols.Api.Services;

public class EngineeringSymbolService : IEngineeringSymbolService
{
    private IEngineeringSymbolRepository _repo;
    
    public EngineeringSymbolService(IConfiguration config, IEngineeringSymbolRepository repo)
    {
        _repo = repo;
    }

    public TryAsync<List<string>> GetSymbolsAsync()
    {
        return _repo.GetAllEngineeringSymbols();
    }

    public TryAsync<EngineeringSymbolDto> GetSymbolAsync(string id)
    {
        return _repo.GetEngineeringSymbolByIdAsync(id);
    }

    public TryAsync<EngineeringSymbolDto> SaveSymbolAsync(EngineeringSymbolDto dto)
    {
        Console.WriteLine($"Save symbol {dto.Id}!");
        return _repo.InsertEngineeringSymbolAsync(dto);
    }
}