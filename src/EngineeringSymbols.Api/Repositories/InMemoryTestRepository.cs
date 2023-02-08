using EngineeringSymbols.Api.Entities;

namespace EngineeringSymbols.Api.Repositories;

public class InMemoryTestRepository : IEngineeringSymbolRepository
{
    private readonly IConfiguration _config;
    public InMemoryTestRepository(IConfiguration config)
    {
        _config = config;
    }
    private List<EngineeringSymbol> InMemoryRepo { get; } = new();

    public TryAsync<IEnumerable<string>> GetAllEngineeringSymbolsAsync() => 
        async () =>
        {
            await Task.Delay(100);
            return InMemoryRepo.Select(dto => dto.Id).ToList();
        };
    
    public TryAsync<EngineeringSymbol> GetEngineeringSymbolByIdAsync(string id) =>
        async () =>
        {
            await Task.Delay(100);
            var symbol = InMemoryRepo.Find(dto => dto.Id == id);

            if (symbol == null)
                throw new RepositoryException(RepositoryOperationError.EntityNotFound);
            
            return symbol;
        };

    public TryAsync<EngineeringSymbol> InsertEngineeringSymbolAsync(EngineeringSymbolCreateDto createDto) =>
        async () =>
        {
            await Task.Delay(100);

            //if (InMemoryRepo.Any(dto => dto.Filename == createDto.Filename))
            //    throw new RepositoryException(RepositoryOperationError.EntityAlreadyExists);

            var rdfKey = _config.GetSection("EngineeringSymbolsPrefix").Value ??
                         "http://example.com/engineering-symbols/";

            var id = RepoUtils.GetRandomString(16);
            
            var newSymbol = new EngineeringSymbol
            {
                Id = id,
                Name = createDto.Name,
                DateTimeCreated = DateTimeOffset.UtcNow,
                DateTimeUpdated = DateTimeOffset.MinValue,
                Filename = createDto.Filename,
                Owner = createDto.Owner,
                Width = createDto.Width,
                Height = createDto.Height,
                GeometryString = createDto.GeometryString,
                SvgString = createDto.SvgString,
                Connectors = createDto.Connectors
            };
            
            InMemoryRepo.Add(newSymbol);

            return newSymbol;
        };
}

public static class RepoUtils
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="size"></param>
    /// <param name="characterPool"></param>
    /// <returns></returns>
    public static string GetRandomString(int size = 12, string? characterPool = null)
    {
        var rnd = new Random();
        var charPool = characterPool ?? "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var result = "";
  
        for (var i = 0; i < size; i++)
        {
            result += charPool[rnd.Next(charPool.Length)];
        }

        return result;
    }
}
