
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

    private Result<T> FusekiRequestErrorResult<T>(HttpResponseMessage httpResponse, string? sparqlQuery = null)
    {
        var reqUri = httpResponse.RequestMessage?.RequestUri?.AbsoluteUri ?? "?";
        var method = httpResponse.RequestMessage?.Method.Method ?? "";
        _logger.LogError(
            "Repository request failed: Status {StatusCode} {ReasonPhrase} (Uri: {Method} {AbsoluteUri})\nSparql Query: \n{SparqlQuery}",
            (int) httpResponse.StatusCode, httpResponse.ReasonPhrase, method, reqUri, sparqlQuery);
        return new Result<T>(new RepositoryException($"Repository request failed: Status {httpResponse.ReasonPhrase}"));
    }

    public TryAsync<IEnumerable<EngineeringSymbol>> GetAllEngineeringSymbolsAsync(bool distinct = true) =>
        async () =>
        {
            var query = SparqlQueries.GetAllSymbolsConstructQuery(distinct);

            var httpResponse =
                await _fuseki.QueryAsync(query, "application/ld+json"); //"application/sparql-results+json" "text/csv"

            if (!httpResponse.IsSuccessStatusCode)
            {
                return FusekiRequestErrorResult<IEnumerable<EngineeringSymbol>>(httpResponse, query);
            }

            var stringContent = await httpResponse.Content.ReadAsStringAsync();
            
            return JsonLdParser.ParseEngineeringSymbols(stringContent);
        };
    
    
    public TryAsync<EngineeringSymbol> GetEngineeringSymbolByIdAsync(string id) =>
        SymbolExistsByIdAsync(id)
            .Bind(exists => new TryAsync<string>(async () =>
                exists 
                    ? SparqlQueries.GetEngineeringSymbolByIdQuery(id) 
                    : new Result<string>(new RepositoryException(RepositoryOperationError.EntityNotFound))
            ))
            .Bind(_getEngineeringSymbolByQueryAsync);
    
    
    public TryAsync<EngineeringSymbol> GetEngineeringSymbolByKeyAsync(string key) =>
        SymbolExistsByKeyAsync(key)
            .Bind(exists => new TryAsync<string>(async () =>
                exists 
                    ? SparqlQueries.GetEngineeringSymbolByKeyQuery(key) 
                    : new Result<string>(new RepositoryException(RepositoryOperationError.EntityNotFound))
            ))
            .Bind(_getEngineeringSymbolByQueryAsync);

    private TryAsync<EngineeringSymbol> _getEngineeringSymbolByQueryAsync(string query) =>
        async () =>
        {
            var httpResponse = await _fuseki.QueryAsync(query, "application/ld+json"); //"text/turtle"

            if (!httpResponse.IsSuccessStatusCode)
            {
                return FusekiRequestErrorResult<EngineeringSymbol>(httpResponse, query);
            }

            var stringContent = await httpResponse.Content.ReadAsStringAsync();

            var parsedSymbols = JsonLdParser.ParseEngineeringSymbols(stringContent);

            return parsedSymbols.First();
            
            //return RdfParser.TurtleToEngineeringSymbol(stringContent)
            //    .Match(symbol => symbol, 
            //        Fail: errors => 
            //        {
            //            foreach (var error in errors)
            //            {
            //                Console.WriteLine(error.Value);
            //            }
   //
            //            return new Result<EngineeringSymbol> (new RepositoryException("Failed to retrieve symbol from store"));
            //        });
        };

    public TryAsync<string> InsertEngineeringSymbolAsync(EngineeringSymbolDto symbol) =>
        async () =>
        {
            var symbolId = Guid.NewGuid().ToString();

            return await EngineeringSymbolValidation.Validate(symbol with {Id = symbolId})
                .MatchAsync(async s => 
                {
                    var query = SparqlQueries.InsertEngineeringSymbolQuery(s);
                    
                    _logger.LogInformation("Sparql Query:\n{SparqlQuery}", query);
                    
                    var httpResponse = await _fuseki.UpdateAsync(query);
                    
                    return httpResponse.IsSuccessStatusCode ? symbolId : FusekiRequestErrorResult<string>(httpResponse, query);
                    
                }, Fail: seq => new Result<string>(new ValidationException(seq)));
        };

    
    public TryAsync<bool> ReplaceEngineeringSymbolAsync(EngineeringSymbolDto dto) =>
        SymbolExistsByIdAsync(dto.Id)
            .Bind(exists => new TryAsync<bool>(async () =>
            {
                if (!exists)
                {
                    return new Result<bool>(new RepositoryException(RepositoryOperationError.EntityNotFound));
                }

                string query = null;  // SparqlQueries.UpdateEngineeringSymbolQuery(id, updateDto);

                if (query == null)
                {
                    return new Result<bool>(new RepositoryException("Invalid update dto"));
                }

                _logger.LogInformation("Sparql Query:\n{SparqlQuery}", query);

                var httpResponse = await _fuseki.UpdateAsync(query);

                return httpResponse.IsSuccessStatusCode
                    ? true
                    : FusekiRequestErrorResult<bool>(httpResponse, query);
            }));

    public TryAsync<bool> DeleteEngineeringSymbolAsync(string id) => 
        SymbolExistsByIdAsync(id)
            .Bind(exists => new TryAsync<bool>(async () => 
            {
                if (!exists)
                {
                    return new Result<bool>(new RepositoryException(RepositoryOperationError.EntityNotFound));
                }
                
                var query = SparqlQueries.DeleteEngineeringSymbolByIdQuery(id);
                
                var httpResponse = await _fuseki.UpdateAsync(query);
                
                return httpResponse.IsSuccessStatusCode 
                    ? true 
                    : FusekiRequestErrorResult<bool>(httpResponse, query);
            }));
    
    private TryAsync<bool> SymbolExistsByIdAsync(string? id) => async () =>
    {
        if (id == null)
        {
            return new Result<bool>(new RepositoryException("Symbol Id was null"));
        }
        
        var query = SparqlQueries.SymbolExistByIdQuery(id);

        var httpResponse = await _fuseki.QueryAsync(query);

        if (!httpResponse.IsSuccessStatusCode)
        {
            return FusekiRequestErrorResult<bool>(httpResponse, query);
        }

        var res = await httpResponse.Content.ReadFromJsonAsync<FusekiAskResponse>();

        if (res == null)
        {
            return new Result<bool>(new RepositoryException("Failed to prove the symbol's existence"));
        }

        return res.Boolean;
    };

    private TryAsync<bool> SymbolExistsByKeyAsync(string? key) => async () =>
    {
        if (key == null)
        {
            return new Result<bool>(new RepositoryException("Symbol Key was null"));
        }
        
        var query = SparqlQueries.SymbolExistByKeyQuery(key);

        var httpResponse = await _fuseki.QueryAsync(query);

        if (!httpResponse.IsSuccessStatusCode)
        {
            return FusekiRequestErrorResult<bool>(httpResponse, query);
        }

        var res = await httpResponse.Content.ReadFromJsonAsync<FusekiAskResponse>();

        if (res == null)
        {
            return new Result<bool>(new RepositoryException("Failed to prove the symbol's existence"));
        }

        return res.Boolean;
    };
}