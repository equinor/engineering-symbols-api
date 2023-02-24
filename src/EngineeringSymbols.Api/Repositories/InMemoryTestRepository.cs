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

    public TryAsync<IEnumerable<EngineeringSymbolListItemResponseDto>> GetAllEngineeringSymbolsAsync() => 
        async () =>
        {
            await Task.Delay(100);
            return InMemoryRepo.Select(dto => new EngineeringSymbolListItemResponseDto {Id = dto.Id, Key = dto.Key}).ToList();
        };
    
    public TryAsync<EngineeringSymbol> GetEngineeringSymbolAsync(string idOrKey) =>
        async () =>
        {
            await Task.Delay(100);
            var symbol = InMemoryRepo.Find(dto => dto.Id == idOrKey);

            if (symbol == null)
                throw new RepositoryException(RepositoryOperationError.EntityNotFound);
            
            return symbol;
        };

    public TryAsync<EngineeringSymbol> GetEngineeringSymbolByKeyAsync(string key)
    {
        throw new NotImplementedException();
    }

    public TryAsync<string> InsertEngineeringSymbolAsync(EngineeringSymbolCreateDto createDto) =>
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
                Key = createDto.Key,
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

            return id;
        };

    public TryAsync<bool> UpdateEngineeringSymbolAsync(string id, EngineeringSymbolUpdateDto updateDto)
    {
        throw new NotImplementedException();
    }

    public TryAsync<bool> UpdateEngineeringSymbolAsync(EngineeringSymbolUpdateDto updateDto)
    {
        throw new NotImplementedException();
    }

    public TryAsync<bool> DeleteEngineeringSymbolAsync(string id)
    {
        throw new NotImplementedException();
    }

    public TryAsync<EngineeringSymbol> UpdateEngineeringSymbolAsync(EngineeringSymbolCreateDto createDto)
    {
        throw new NotImplementedException();
    }
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
