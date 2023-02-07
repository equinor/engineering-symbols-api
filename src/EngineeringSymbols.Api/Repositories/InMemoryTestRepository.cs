using LanguageExt.SomeHelp;

namespace EngineeringSymbols.Api.Repositories;

public class InMemoryTestRepository : IEngineeringSymbolRepository
{
    private List<EngineeringSymbolDto> InMemoryRepo { get; } = new();

    public TryAsync<List<string>> GetAllEngineeringSymbols() => 
        async () =>
        {
            await Task.Delay(100);
            return InMemoryRepo.Select(dto => dto.Id).ToList();
        };
    
    public TryAsync<EngineeringSymbolDto> GetEngineeringSymbolByIdAsync(string id) =>
        async () =>
        {
            await Task.Delay(100);
            var sym = InMemoryRepo.Find(dto => dto.Id == id);

            if (sym == null)
                throw new RepositoryException("Not found");
            
            return sym;
        };

    public TryAsync<EngineeringSymbolDto> InsertEngineeringSymbolAsync(EngineeringSymbolDto symbol) =>
        async () =>
        {
            await Task.Delay(100);

            if (InMemoryRepo.Any(dto => dto.Id == symbol.Id))
                throw new RepositoryException("Already exists");

            var newSymbol = symbol with {Id = Guid.NewGuid().ToString()};

            InMemoryRepo.Add(newSymbol);

            return newSymbol;
        };
}