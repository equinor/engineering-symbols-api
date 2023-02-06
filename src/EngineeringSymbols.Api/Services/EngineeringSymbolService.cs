using EngineeringSymbols.Api.Models;

namespace EngineeringSymbols.Api.Services;

public class EngineeringSymbolService : IEngineeringSymbolService
{
    public EngineeringSymbolService(IConfiguration config)
    {
        
    }
    
    public async Task<Option<EngineeringSymbolDto>> SaveSymbolAsync(EngineeringSymbolDto dto)
    {
        Console.WriteLine($"Save symbol {dto.Id}!");

        return Optional(dto with { });
    }
}