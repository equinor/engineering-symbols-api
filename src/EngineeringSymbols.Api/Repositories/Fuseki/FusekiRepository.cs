using EngineeringSymbols.Api.Utils;
using EngineeringSymbols.Tools.Entities;

namespace EngineeringSymbols.Api.Repositories.Fuseki;

public class FusekiRepository : IEngineeringSymbolRepository
{
    private readonly IFusekiService _fuseki;
    
    public FusekiRepository(IFusekiService fuseki)
    {
        _fuseki = fuseki;
    }

    public TryAsync<IEnumerable<EngineeringSymbolListLatestItemResponseDto>> GetAllEngineeringSymbolsAsync() =>
        async () =>
        {
            var query = SparqlQueries.GetAllSymbolsDistinctQuery2();

            var httpResponse = await _fuseki.QueryAsync(query, "text/csv");

            var stringContent = await httpResponse.Content.ReadAsStringAsync();

            var symbolArray = stringContent
                .Split("\n")
                .Select(s => s.Replace("\r", "").Trim().Split(","))
                .Filter(s => s.Length == 3)
                .Skip(1)
                .Select(s => new EngineeringSymbolListLatestItemResponseDto
                {
                    Key = s[1],
                    IdLatestVersion = s[0].Split("/").Last(),
                    Versions = int.Parse(s[2])
                })
                .ToArray();

            return symbolArray;
        };
    
    public TryAsync<IEnumerable<EngineeringSymbolListItemResponseDto>> GetAllEngineeringSymbolsIncludeAllVersionsAsync() =>
        async () =>
        {
            const string query = SparqlQueries.GetAllSymbolsQuery;

            var httpResponse = await _fuseki.QueryAsync(query, "text/csv");

            var stringContent = await httpResponse.Content.ReadAsStringAsync();

            var symbolArray = stringContent
                .Split("\n")
                .Select(s => s.Replace("\r", "").Trim().Split(","))
                .Filter(s => s.Length == 2)
                .Skip(1)
                .Select(s => new EngineeringSymbolListItemResponseDto
                {
                    Id = s[0].Split("/").Last(),
                    Key = s[1],
                })
                .ToArray();

            return symbolArray;
        };

    public TryAsync<EngineeringSymbol> GetEngineeringSymbolAsync(string idOrKey) =>
        async () =>
        {
            string query;

            if (await SymbolExistsByIdAsync(idOrKey))
            {
                query = SparqlQueries.GetEngineeringSymbolByIdQuery(idOrKey);
            } else if (await SymbolExistsByKeyAsync(idOrKey))
            {
                query = SparqlQueries.GetEngineeringSymbolByKeyQuery(idOrKey);
            }
            else
            {
                throw new RepositoryException(RepositoryOperationError.EntityNotFound);
            }

            var httpResponse = await _fuseki.QueryAsync(query, "text/turtle");
            var stringContent = await httpResponse.Content.ReadAsStringAsync();
            Console.WriteLine(stringContent);
            var symbolV = RdfParser.TurtleToEngineeringSymbol(stringContent);

            return symbolV.Match(symbol => symbol, seq =>
            {
                foreach (var error in seq)
                {
                    Console.WriteLine(error.Value);
                }

                throw new RepositoryException("Failed to retrieve symbol from store");
            });
        };

    public TryAsync<string> InsertEngineeringSymbolAsync(EngineeringSymbolCreateDto createDto) =>
        async () =>
        {
            var symbolId = Guid.NewGuid().ToString(); //RepoUtils.GetRandomString();
            var query = SparqlQueries.InsertEngineeringSymbolQuery(symbolId, createDto);
            
            Console.WriteLine("   ---  QUERY  ---");
            Console.WriteLine(query);

            HttpResponseMessage fusekiResponse;
            
            try
            {
                fusekiResponse = await _fuseki.UpdateAsync(query);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw new RepositoryException(e.Message);
            }

            if (!fusekiResponse.IsSuccessStatusCode)
            {
                throw new RepositoryException(fusekiResponse.ReasonPhrase);
            }
            
            return symbolId;
        };

    public TryAsync<bool> UpdateEngineeringSymbolAsync(string id, EngineeringSymbolUpdateDto updateDto) => 
        async () => 
        {
            if (!await SymbolExistsByIdAsync(id))
            {
                throw new RepositoryException(RepositoryOperationError.EntityNotFound);
            }
            
            var query = SparqlQueries.UpdateEngineeringSymbolQuery(id, updateDto);

            if (query == null)
            {
                throw new RepositoryException("Invalid update dto");
            }
            
            Console.WriteLine("   ---  PATCH QUERY  ---");
            Console.WriteLine(query);
            
            HttpResponseMessage fusekiResponse;

            try
            {
                fusekiResponse = await _fuseki.UpdateAsync(query);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw new RepositoryException(e.Message);
            }

            if (!fusekiResponse.IsSuccessStatusCode)
            {
                throw new RepositoryException("Failed to update symbol");
            }

            return true;
        };

    public TryAsync<bool> DeleteEngineeringSymbolAsync(string id) =>
        async () =>
        {
            var query = SparqlQueries.DeleteEngineeringSymbolByIdQuery(id);

            if (!await SymbolExistsByIdAsync(id))
            {
                throw new RepositoryException(RepositoryOperationError.EntityNotFound);
            }
            
            HttpResponseMessage fusekiResponse;

            try
            {
                fusekiResponse = await _fuseki.UpdateAsync(query);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw new RepositoryException(e.Message);
            }

            if (!fusekiResponse.IsSuccessStatusCode)
            {
                throw new RepositoryException("Failed to delete symbol");
            }

            return true;
        };

    private async Task<bool> SymbolExistsByIdAsync(string id)
    {
        var query = SparqlQueries.SymbolExistByIdQuery(id);
        
        HttpResponseMessage fusekiResponse;
        try
        {
            fusekiResponse = await _fuseki.QueryAsync(query);
        }
        catch (Exception e)
        {
            throw new RepositoryException(e.Message);
        }

        var res = await fusekiResponse.Content.ReadFromJsonAsync<FusekiAskResponse>();

        if (res == null)
        {
            throw new RepositoryException("Failed to prove the symbol's existence");
        }
        
        return res.Boolean;
    }
    
    private async Task<bool> SymbolExistsByKeyAsync(string key)
    {
        var query = SparqlQueries.SymbolExistByKeyQuery(key);
        
        HttpResponseMessage fusekiResponse;
        try
        {
            fusekiResponse = await _fuseki.QueryAsync(query);
        }
        catch (Exception e)
        {
            throw new RepositoryException(e.Message);
        }

        var res = await fusekiResponse.Content.ReadFromJsonAsync<FusekiAskResponse>();

        if (res == null)
        {
            throw new RepositoryException("Failed to prove the symbol's existence");
        }
        
        return res.Boolean;
    }

    private async Task<bool> SymbolExistsAsync(string idOrKey)
    {
        if (await SymbolExistsByIdAsync(idOrKey))
            return true;
        return await SymbolExistsByKeyAsync(idOrKey);
    }
}