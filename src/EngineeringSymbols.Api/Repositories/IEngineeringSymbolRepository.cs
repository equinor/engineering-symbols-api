namespace EngineeringSymbols.Api.Repositories;

public interface IEngineeringSymbolRepository
{
    TryAsync<List<string>> GetAllEngineeringSymbols();
    TryAsync<EngineeringSymbolDto> GetEngineeringSymbolByIdAsync(string id);
    TryAsync<EngineeringSymbolDto> InsertEngineeringSymbolAsync(EngineeringSymbolDto symbol);
}