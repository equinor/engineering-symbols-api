using EngineeringSymbols.Tools.Entities;
using EngineeringSymbols.Tools.Models;
using EngineeringSymbols.Tools.Validation;

namespace EngineeringSymbols.Api.Repositories.Fuseki;

public class FusekiRepository : IEngineeringSymbolRepository
{
    private readonly ILogger _logger;
    private readonly IFusekiService _fuseki;
    
    public FusekiRepository(IFusekiService fuseki, ILoggerFactory loggerFactory)
    {
        _fuseki = fuseki;
        _logger = loggerFactory.CreateLogger("FusekiRepository");
    }

    private void ThrowOnFusekiRequestError(HttpResponseMessage httpResponse, string? sparqlQuery = null)
    {
        if (httpResponse.IsSuccessStatusCode) return;
        var reqUri = httpResponse.RequestMessage?.RequestUri?.AbsoluteUri ?? "?";
        var method = httpResponse.RequestMessage?.Method.Method ?? "";
        _logger.LogError("Repository request failed: Status {StatusCode} {ReasonPhrase} (Uri: {Method} {AbsoluteUri})\nSparql Query: \n{SparqlQuery}", (int)httpResponse.StatusCode, httpResponse.ReasonPhrase, method, reqUri, sparqlQuery);
        throw new RepositoryException($"Repository request failed: Status {httpResponse.ReasonPhrase}");
    }
    
    public TryAsync<IEnumerable<IEngineeringSymbolResponseDto>> GetAllEngineeringSymbolsAsync() =>
        async () =>
        {
            var query = SparqlQueries.GetAllSymbolsDistinctQuery2();

            var httpResponse = await _fuseki.QueryAsync(query, "text/csv");

            ThrowOnFusekiRequestError(httpResponse);
            
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
                    NumberOfVersions = int.Parse(s[2])
                })
                .ToArray();

            return symbolArray;
        };
    
    public TryAsync<IEnumerable<IEngineeringSymbolResponseDto>> GetAllEngineeringSymbolsIncludeAllVersionsAsync() =>
        async () =>
        {
            const string query = SparqlQueries.GetAllSymbolsQuery;

            var httpResponse = await _fuseki.QueryAsync(query, "text/csv");

            ThrowOnFusekiRequestError(httpResponse, query);
            
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


    public TryAsync<EngineeringSymbol> GetEngineeringSymbolByIdAsync(string id) =>
        async () =>
        {
            if (!await SymbolExistsByIdAsync(id))
            {
                throw new RepositoryException(RepositoryOperationError.EntityNotFound);
            }
            
            return await _getEngineeringSymbolByQueryAsync(SparqlQueries.GetEngineeringSymbolByIdQuery(id));
        };
        
    
    public TryAsync<EngineeringSymbol> GetEngineeringSymbolByKeyAsync(string key) =>
        async () =>
        {
            if (!await SymbolExistsByKeyAsync(key))
            {
                throw new RepositoryException(RepositoryOperationError.EntityNotFound);
            }
            
            return await _getEngineeringSymbolByQueryAsync(SparqlQueries.GetEngineeringSymbolByKeyQuery(key));
        };
    
    private async Task<EngineeringSymbol> _getEngineeringSymbolByQueryAsync(string query)
    {
            var httpResponse = await _fuseki.QueryAsync(query, "text/turtle");
            
            ThrowOnFusekiRequestError(httpResponse, query);
            
            var stringContent = await httpResponse.Content.ReadAsStringAsync();

            var symbolV = RdfParser.TurtleToEngineeringSymbol(stringContent);

            return symbolV.Match(symbol => symbol, seq =>
            {
                foreach (var error in seq)
                {
                    Console.WriteLine(error.Value);
                }

                throw new RepositoryException("Failed to retrieve symbol from store");
            });
    }

    public TryAsync<string> InsertEngineeringSymbolAsync(EngineeringSymbolCreateDto createDto) =>
        async () =>
        {
            var symbolId = Guid.NewGuid().ToString(); //RepoUtils.GetRandomString();
            var query = SparqlQueries.InsertEngineeringSymbolQuery(symbolId, createDto);
            
            Console.WriteLine("   ---  QUERY  ---");
            Console.WriteLine(query);
            
            var fusekiResponse = await _fuseki.UpdateAsync(query);
            
            ThrowOnFusekiRequestError(fusekiResponse, query);
            
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

            var fusekiResponse = await _fuseki.UpdateAsync(query);
            ThrowOnFusekiRequestError(fusekiResponse, query);

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
            
            var fusekiResponse = await _fuseki.UpdateAsync(query);
            ThrowOnFusekiRequestError(fusekiResponse, query);
            
            return true;
        };

    private async Task<bool> SymbolExistsByIdAsync(string id)
    {
        
        
        var query = SparqlQueries.SymbolExistByIdQuery(id);
        
        var fusekiResponse = await _fuseki.QueryAsync(query);
        ThrowOnFusekiRequestError(fusekiResponse, query);

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
        
        var fusekiResponse = await _fuseki.QueryAsync(query);
        ThrowOnFusekiRequestError(fusekiResponse, query);

        var res = await fusekiResponse.Content.ReadFromJsonAsync<FusekiAskResponse>();

        if (res == null)
        {
            throw new RepositoryException("Failed to prove the symbol's existence");
        }
        
        return res.Boolean;
    }
}