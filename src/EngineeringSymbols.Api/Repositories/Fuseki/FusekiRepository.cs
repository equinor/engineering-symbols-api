using System.Globalization;
using System.Text;
using EngineeringSymbols.Api.Endpoints;
using EngineeringSymbols.Api.Entities;
using EngineeringSymbols.Api.Utils;
using LanguageExt.Pipes;
using LanguageExt.Pretty;
using VDS.RDF;
using VDS.RDF.Parsing;

namespace EngineeringSymbols.Api.Repositories.Fuseki;

public static class RdfConst
{
    public const string BaseIri = "https://rdf.equinor.com";
    
    public const string IndividualPrefix = "symbol";
    
    public const string EngSymOntologyPrefix = "es";

    // Predicate base IRIs

    public const string EngSymOntologyIri = BaseIri + "/ontology/engineering-symbol/v1#";
    
    public const string SymbolTypeIri = EngSymOntologyIri + "Symbol";
    public const string ConnectorTypeIri = EngSymOntologyIri + "Connector";
 
    
    public const string SymbolIri = BaseIri + "/engineering-symbols/";
    
    public const string RdfIri = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";
    public const string RdfsIri = "http://www.w3.org/2000/01/rdf-schema#";
    
    // Prefixes (turtle)
    
    public const string RdfPrefix = $"PREFIX rdf: <{RdfIri}>";
    public const string RdfsPrefix = $"PREFIX rdfs: <{RdfsIri}>";
    
    public const string EngSymPrefix = $"PREFIX {EngSymOntologyPrefix}: <{EngSymOntologyIri}>";
    public const string SymbolPrefix = $"PREFIX {IndividualPrefix}: <{SymbolIri}>";
    
    public const string AllPrefixes = $$"""
                                        {{RdfPrefix}}
                                        {{RdfsPrefix}}
                                        {{EngSymPrefix}}
                                        {{SymbolPrefix}}
                                        """;
    
}

public static class ESProp
{
    public const string IsType = "type";
    public const string IsTypeIri = $"{RdfConst.RdfIri}{IsType}";
    public const string IsTypeIriPrefix = $"rdf:{IsType}";
    
    public const string HasLabel = "label";
    public const string HasLabelIri = $"{RdfConst.RdfsIri}{HasLabel}";
    public const string HasLabelIriPrefix = $"rdfs:{HasLabel}";
    
    public const string HasEngSymKey = "key";
    public const string HasEngSymKeyIri = $"{RdfConst.EngSymOntologyIri}{HasEngSymKey}";
    public const string HasEngSymKeyIriPrefix = $"{RdfConst.EngSymOntologyPrefix}:{HasEngSymKey}";
    
    public const string HasName = "hasName";
    public const string HasNameIri = $"{RdfConst.EngSymOntologyIri}{HasName}";
    public const string HasNameIriPrefix = $"{RdfConst.EngSymOntologyPrefix}:{HasName}";
    
    public const string HasDescription = "hasDescription";
    public const string HasDescriptionIri = $"{RdfConst.EngSymOntologyIri}{HasDescription}";
    public const string HasDescriptionIriPrefix = $"{RdfConst.EngSymOntologyPrefix}:{HasDescription}";
    
    public const string HasSourceFilename = "hasSourceFilename";
    public const string HasSourceFilenameIri = $"{RdfConst.EngSymOntologyIri}{HasSourceFilename}";
    public const string HasSourceFilenameIriPrefix = $"{RdfConst.EngSymOntologyPrefix}:{HasSourceFilename}";
    
    public const string HasGeometry = "hasGeometry";
    public const string HasGeometryIri = $"{RdfConst.EngSymOntologyIri}{HasGeometry}";
    public const string HasGeometryIriPrefix = $"{RdfConst.EngSymOntologyPrefix}:{HasGeometry}";
    
    public const string HasSvg = "hasSvg";
    public const string HasSvgIri = $"{RdfConst.EngSymOntologyIri}{HasSvg}";
    public const string HasSvgIriPrefix = $"{RdfConst.EngSymOntologyPrefix}:{HasSvg}";
    
    public const string HasSvgBase64 = "hasSvgBase64";
    public const string HasSvgBase64Iri = $"{RdfConst.EngSymOntologyIri}{HasSvgBase64}";
    public const string HasSvgBase64IriPrefix = $"{RdfConst.EngSymOntologyPrefix}:{HasSvgBase64}";
    
    public const string HasWidth = "width";
    public const string HasWidthIri = $"{RdfConst.EngSymOntologyIri}{HasWidth}";
    public const string HasWidthIriPrefix = $"{RdfConst.EngSymOntologyPrefix}:{HasWidth}";
    
    public const string HasHeight = "height";
    public const string HasHeightIri = $"{RdfConst.EngSymOntologyIri}{HasHeight}";
    public const string HasHeightIriPrefix = $"{RdfConst.EngSymOntologyPrefix}:{HasHeight}";
    
    public const string HasOwner = "owner";
    public const string HasOwnerIri = $"{RdfConst.EngSymOntologyIri}{HasOwner}";
    public const string HasOwnerIriPrefix = $"{RdfConst.EngSymOntologyPrefix}:{HasOwner}";
    
    public const string HasConnector = "hasConnector";
    public const string HasConnectorIri = $"{RdfConst.EngSymOntologyIri}{HasConnector}";
    public const string HasConnectorIriPrefix = $"{RdfConst.EngSymOntologyPrefix}:{HasConnector}";
    
    public const string HasDateCreated = "dateCreated";
    public const string HasDateCreatedIri = $"{RdfConst.EngSymOntologyIri}{HasDateCreated}";
    public const string HasDateCreatedIriPrefix = $"{RdfConst.EngSymOntologyPrefix}:{HasDateCreated}";
    
    public const string HasDateUpdated = "dateUpdated";
    public const string HasDateUpdatedIri = $"{RdfConst.EngSymOntologyIri}{HasDateUpdated}";
    public const string HasDateUpdatedIriPrefix = $"{RdfConst.EngSymOntologyPrefix}:{HasDateUpdated}";
    
    public const string HasPositionX = "positionX";
    public const string HasPositionXIri = $"{RdfConst.EngSymOntologyIri}{HasPositionX}";
    public const string HasPositionXIriPrefix = $"{RdfConst.EngSymOntologyPrefix}:{HasPositionX}";
    
    public const string HasPositionY = "positionY";
    public const string HasPositionYIri = $"{RdfConst.EngSymOntologyIri}{HasPositionY}";
    public const string HasPositionYIriPrefix = $"{RdfConst.EngSymOntologyPrefix}:{HasPositionY}";
    
    public const string HasDirection = "direction";
    public const string HasDirectionYIri = $"{RdfConst.EngSymOntologyIri}{HasDirection}";
    public const string HasDirectionYIriPrefix = $"{RdfConst.EngSymOntologyPrefix}:{HasDirection}";
}

//public static class ESPred
//{
//    public const string HasLabel = $"rdfs:label";
//    public const string HasName = $"{RdfConst.Prop}:hasName";
//    public const string HasGeometry = $"{RdfConst.Prop}:hasGeometry";
//    public const string HasSvg = $"{RdfConst.Prop}:hasSvg";
//    public const string HasWidth = $"{RdfConst.Prop}:width";
//    public const string HasHeight = $"{RdfConst.Prop}:height";
//    public const string HasOwner = $"{RdfConst.Prop}:owner";
//    public const string HasConnector = $"{RdfConst.Prop}:hasConnector";
//    public const string HasDateCreated = $"{RdfConst.Prop}:dateCreated";
//    public const string HasDateUpdated = $"{RdfConst.Prop}:dateUpdated";
//    public const string HasPositionX = $"{RdfConst.Prop}:positionX";
//    public const string HasPositionY = $"{RdfConst.Prop}:positionY";
//    public const string HasDirection = $"{RdfConst.Prop}:direction";
//    
//}

public class FusekiRepository : IEngineeringSymbolRepository
{
    private readonly IFusekiService _fuseki;
    
    public FusekiRepository(IFusekiService fuseki)
    {
        _fuseki = fuseki;
    }

    public TryAsync<IEnumerable<EngineeringSymbolListItemResponseDto>> GetAllEngineeringSymbolsAsync() =>
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
                .Select(s => new EngineeringSymbolListItemResponseDto {Id = s[0].Split("/").Last(), Key = s[1]})
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