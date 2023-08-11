using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using AngleSharp.Dom;
using EngineeringSymbols.Tools.Constants;
using EngineeringSymbols.Tools.Entities;
using EngineeringSymbols.Tools.Models;
using EngineeringSymbols.Tools.Validation;
using JsonSerializer = System.Text.Json.JsonSerializer;


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

    public TryAsync<IEnumerable<IEngineeringSymbolResponseDto>> GetAllEngineeringSymbolsAsync(bool distinct = true) =>
        async () =>
        {
            var query = SparqlQueries.GetAllSymbolsConstructQuery(distinct);

            var httpResponse =
                await _fuseki.QueryAsync(query, "application/ld+json"); //"application/sparql-results+json" "text/csv"

            if (!httpResponse.IsSuccessStatusCode)
            {
                return FusekiRequestErrorResult<IEnumerable<IEngineeringSymbolResponseDto>>(httpResponse, query);
            }

            var stringContent = await httpResponse.Content.ReadAsStringAsync();

            var symbolsJsonDom = JsonSerializer.Deserialize<JsonObject>(stringContent);

            var symbols = new List<IEngineeringSymbolResponseDto>();

            if (symbolsJsonDom.ContainsKey("@id") && symbolsJsonDom.ContainsKey("@graph"))
            {
                symbols.Add(JsonObjectToEngineeringSymbol(symbolsJsonDom));
            }
            else if (symbolsJsonDom.ContainsKey("@graph"))
            {
                if (symbolsJsonDom["@graph"] is not JsonArray symbolGraphs)
                {
                    throw new Exception("");
                }

                foreach (var symbolGraph in symbolGraphs)
                {
                    if (symbolGraph == null) continue;

                    symbols.Add(JsonObjectToEngineeringSymbol(symbolGraph));
                }
            }
            
            return symbols;
        };


    public static EngineeringSymbolCompleteResponseDto JsonObjectToEngineeringSymbol(JsonNode jsonNode)
    {
        var jsonObject = jsonNode.AsObject();
        
        if (jsonObject["@graph"] is not JsonArray graph)
        {
            throw new Exception("");
        }

        EngineeringSymbolCompleteResponseDto? symbolParsed = null;
        var connectors = new List<EngineeringSymbolConnector>();
        
        // The nodes are either connectors or a symbol
        foreach (var node in graph)
        {
            if (node is not JsonObject ob) continue;

            if (ob.TryGetPropertyValue("@type", out var typeNode) && typeNode != null)
            {
                var typeIri = typeNode.ToString();

                if (typeIri.StartsWith(RdfConst.EngSymOntologyPrefix + ":"))
                {
                    typeIri = RdfConst.EngSymOntologyIri + typeIri.Split(":").Last().Trim();
                }

                switch (typeIri)
                {
                    case RdfConst.SymbolTypeIri:
                        symbolParsed = ParseSymbolObject(ob);
                        break;
                    case RdfConst.ConnectorTypeIri:
                        connectors.Add(ParseConnectorObject(ob));
                        break;
                }
            }
        }

        if (symbolParsed == null)
        {
            throw new Exception("");
        }

        symbolParsed.Connectors.AddRange(connectors);

        return symbolParsed;
    }

    public static T GetSymbolProp<T>(JsonObject obj, string prop)
    {
        obj.TryGetPropertyValue(prop, out var node);

        if (node == null)
        {
            throw new Exception("Prop not found");
        }
        
        if (typeof(T) == typeof(string))
        {
            return (T) (object) node.ToString();
        }
        
        if (typeof(T) == typeof(double))
        {
            if (node is JsonObject jobj && jobj.ContainsKey("@value"))
            {
                double.TryParse(jobj["@value"].ToString(), CultureInfo.InvariantCulture, out var parsedDouble);
            
                return (T) (object) parsedDouble;
            }
        }
        
        if (typeof(T) == typeof(int))
        {
            if (node is JsonObject jobj && jobj.ContainsKey("@value"))
            {
                int.TryParse(jobj["@value"].ToString(), null, out var parsedInt);
            
                return (T) (object) parsedInt;
            }
        } 
        
        if (typeof(T) == typeof(DateTimeOffset))
        {
            if (node is JsonObject dobj && dobj.ContainsKey("@value"))
            {
                DateTimeOffset.TryParseExact(dobj["@value"].ToString(), "o", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDateTime);
                return (T) (object) parsedDateTime;
            }
        } 
        
        return default;
    }
    
    public static EngineeringSymbolCompleteResponseDto ParseSymbolObject(JsonObject obj)
    {
        return new EngineeringSymbolCompleteResponseDto
        {
            Id = GetSymbolProp<string>(obj, ESProp.HasEngSymIdIriPrefix),
            Key = GetSymbolProp<string>(obj, ESProp.HasEngSymKeyIriPrefix),
            Status = GetSymbolProp<string>(obj, ESProp.HasStatusIriPrefix),
            Description = GetSymbolProp<string>(obj, ESProp.HasDescriptionIriPrefix),
            DateTimeCreated = GetSymbolProp<DateTimeOffset>(obj, ESProp.HasDateCreatedIriPrefix),
            DateTimeUpdated = GetSymbolProp<DateTimeOffset>(obj, ESProp.HasDateUpdatedIriPrefix),
            Owner = GetSymbolProp<string>(obj, ESProp.HasOwnerIriPrefix),
            Filename = GetSymbolProp<string>(obj, ESProp.HasSourceFilenameIriPrefix),
            Geometry = GetSymbolProp<string>(obj, ESProp.HasGeometryIriPrefix),
            Width = GetSymbolProp<double>(obj, ESProp.HasWidthIriPrefix),
            Height = GetSymbolProp<double>(obj, ESProp.HasHeightIriPrefix),
            Connectors = new List<EngineeringSymbolConnector>()
        };
    }
    
    public static EngineeringSymbolConnector ParseConnectorObject(JsonObject obj)
    {
        return new EngineeringSymbolConnector
        {
            Id = GetSymbolProp<string>(obj, ESProp.HasNameIriPrefix),
            RelativePosition = new Point
            {
                X = GetSymbolProp<double>(obj, ESProp.HasPositionXIriPrefix),
                Y = GetSymbolProp<double>(obj, ESProp.HasPositionYIriPrefix)
            },
            Direction = GetSymbolProp<int>(obj, ESProp.HasDirectionIriPrefix)
        };
    }

    
    public TryAsync<IEnumerable<IEngineeringSymbolResponseDto>> GetAllEngineeringSymbolsIncludeAllVersionsAsync() =>
        async () =>
        {
            var query = SparqlQueries.GetAllSymbolsQuery();

            var httpResponse = await _fuseki.QueryAsync(query, "text/csv");

            if (!httpResponse.IsSuccessStatusCode)
            {
                return FusekiRequestErrorResult<IEnumerable<IEngineeringSymbolResponseDto>>(httpResponse, query);
            }
            
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
            var httpResponse = await _fuseki.QueryAsync(query, "text/turtle");

            if (!httpResponse.IsSuccessStatusCode)
            {
                return FusekiRequestErrorResult<EngineeringSymbol>(httpResponse, query);
            }

            var stringContent = await httpResponse.Content.ReadAsStringAsync();

            return RdfParser.TurtleToEngineeringSymbol(stringContent)
                .Match(symbol => symbol, 
                    Fail: errors => 
                    {
                        foreach (var error in errors)
                        {
                            Console.WriteLine(error.Value);
                        }

                        return new Result<EngineeringSymbol> (new RepositoryException("Failed to retrieve symbol from store"));
                    });
        };

    public TryAsync<string> InsertEngineeringSymbolAsync(EngineeringSymbolCreateDto createDto) =>
        async () =>
        {
            var symbolId = Guid.NewGuid().ToString(); //RepoUtils.GetRandomString();
            var query = SparqlQueries.InsertEngineeringSymbolQuery(symbolId, createDto);
            
            _logger.LogInformation("Sparql Query:\n{SparqlQuery}", query);
            
            var httpResponse = await _fuseki.UpdateAsync(query);
            
            return httpResponse.IsSuccessStatusCode ? symbolId : FusekiRequestErrorResult<string>(httpResponse, query);
        };

    public TryAsync<bool> UpdateEngineeringSymbolAsync(string id, EngineeringSymbolUpdateDto updateDto) =>
        SymbolExistsByIdAsync(id)
            .Bind(exists => new TryAsync<bool>(async () =>
            {
                if (!exists)
                {
                    return new Result<bool>(new RepositoryException(RepositoryOperationError.EntityNotFound));
                }

                var query = SparqlQueries.UpdateEngineeringSymbolQuery(id, updateDto);

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
    
    private TryAsync<bool> SymbolExistsByIdAsync(string id) => async () =>
    {
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

    private TryAsync<bool> SymbolExistsByKeyAsync(string key) => async () =>
    {
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