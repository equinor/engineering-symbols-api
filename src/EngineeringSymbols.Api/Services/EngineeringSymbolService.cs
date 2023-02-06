using EngineeringSymbols.Api.Models;

namespace EngineeringSymbols.Api.Services;

public class EngineeringSymbolService : IEngineeringSymbolService
{
    public EngineeringSymbolService(IConfiguration config)
    {
        
    }
    
    public async Task<EngineeringSymbolDto> SaveSymbol(EngineeringSymbolDto dto)
    {
        
        Console.WriteLine($"Save symbol {dto.Id}!");

        return dto with {};
    }
}