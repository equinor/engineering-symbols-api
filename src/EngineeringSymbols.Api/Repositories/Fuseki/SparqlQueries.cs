using System.Globalization;
using System.Text;
using EngineeringSymbols.Tools.Constants;

namespace EngineeringSymbols.Api.Repositories.Fuseki;

public static class SparqlQueries
{
    public const string GetAllSymbolsQuery = $$"""
                    {{RdfConst.EngSymPrefix}}
                    SELECT ?symbolGraph ?key WHERE { 
                        GRAPH ?symbolGraph { ?s {{ESProp.HasEngSymKeyIriPrefix}} ?key } 
                    }
                    """;
    
    public const string GetAllSymbolsQuery2 = $$"""
                    PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>
                    SELECT ?symbolIri WHERE { ?symbolIri rdf:type <{{RdfConst.SymbolTypeIri}}> }
                    """;


    public static string GetAllSymbolsDistinctQuery()
    {
        // Include date: SELECT ?symbolGraph ?key ?latestDateCreated WHERE ...
        return $$"""
                {{RdfConst.EngSymPrefix}}
                {{RdfConst.SymbolPrefix}}

                SELECT ?symbolGraph ?key
                WHERE {
                    GRAPH ?symbolGraph { 
                        ?s1 {{ESProp.HasEngSymKeyIriPrefix}} ?key .
                        ?s1 {{ESProp.HasDateCreatedIriPrefix}} ?latestDateCreated . 
                    }
                  
                    {
                        SELECT ?key (MAX(?dc) AS ?latestDateCreated) 
                        WHERE {
                            GRAPH ?g { 
                                ?s2 {{ESProp.HasEngSymKeyIriPrefix}} ?key .
                                ?s2 {{ESProp.HasDateCreatedIriPrefix}} ?dc .
                            }
                        }
                        GROUP BY ?key
                    }
                }
                """;
    }
    
    public static string GetAllSymbolsDistinctQuery2()
    {
        return $$"""
                {{RdfConst.XsdPrefix}}
                {{RdfConst.EngSymPrefix}}
                {{RdfConst.SymbolPrefix}}

                SELECT ?symbolGraph ?key ?numVersions
                WHERE {
                    GRAPH ?symbolGraph { 
                        ?s1 {{ESProp.HasEngSymKeyIriPrefix}} ?key .
                        ?s1 {{ESProp.HasDateCreatedIriPrefix}} ?latestDateCreated . 
                    }
                  
                    {
                        SELECT ?key (MAX(?dc) AS ?latestDateCreated) (COUNT(?g) AS ?numVersions)
                        WHERE {
                            GRAPH ?g { 
                                ?s2 {{ESProp.HasEngSymKeyIriPrefix}} ?key .
                                ?s2 {{ESProp.HasDateCreatedIriPrefix}} ?dc .
                            }
                        }
                        GROUP BY ?key
                    }
                }
                """;
    }
    

    
    public static string SymbolExistByIdQuery(string id)
    {
        return $$"""
                {{RdfConst.SymbolPrefix}}
                ASK WHERE { 
                    GRAPH {{RdfConst.IndividualPrefix}}:{{id}} { ?s ?p ?o }
                }
                """;
    }
    
    public static string SymbolExistByKeyQuery(string key)
    {
        return $$"""
                {{RdfConst.EngSymPrefix}}
                ASK WHERE { 
                    GRAPH ?g { 
                        ?s {{ESProp.HasEngSymKeyIriPrefix}} "{{key}}" 
                    }
                }
                """;
    }
    
    public static string GetEngineeringSymbolByIdQuery(string id)
    {
        return $$"""
                {{RdfConst.XsdPrefix}}
                {{RdfConst.SymbolPrefix}}
                {{RdfConst.EngSymPrefix}}

                CONSTRUCT 
                {
                    ?s ?o ?p 
                }
                WHERE 
                {
                    GRAPH {{RdfConst.IndividualPrefix}}:{{id}} { ?s ?o ?p }
                }
                """;
    }
    
    public static string GetEngineeringSymbolByKeyQuery(string key)
    {
        return $$"""
                {{RdfConst.XsdPrefix}}
                {{RdfConst.RdfsPrefix}}
                {{RdfConst.SymbolPrefix}}
                {{RdfConst.EngSymPrefix}}

                CONSTRUCT 
                {
                    ?s ?o ?p 
                }
                WHERE 
                {
                    GRAPH ?g 
                    { 
                        ?s ?o ?p .
                        ?ss {{ESProp.HasEngSymKeyIriPrefix}} "{{key}}" .
                    }
                }
                """;
    }
    
    public static string DeleteEngineeringSymbolByIdQuery(string id)
    {
        return $"""
                {RdfConst.SymbolPrefix}
                DROP GRAPH {RdfConst.IndividualPrefix}:{id}
                """;
    }
    
    
    public static string InsertEngineeringSymbolQuery(string symbolId, EngineeringSymbolCreateDto createDto)
    {
        var nfi = new NumberFormatInfo {NumberDecimalSeparator = "."};
        var sub = $"{RdfConst.IndividualPrefix}:{symbolId}";
        
        var connectorTurtle = createDto.Connectors.Map(connector =>
        {
            var cIri = $"{RdfConst.IndividualPrefix}:{symbolId}_C_{connector.Id}";
                
            return $"""
                            {sub} {ESProp.HasConnectorIriPrefix} {cIri} .
                            {cIri} {ESProp.IsTypeIriPrefix} <{RdfConst.ConnectorTypeIri}> .
                            {cIri} {ESProp.HasNameIriPrefix} "{connector.Id}" .
                            {cIri} {ESProp.HasPositionXIriPrefix} "{connector.RelativePosition.X.ToString(nfi)}" .
                            {cIri} {ESProp.HasPositionYIriPrefix} "{connector.RelativePosition.Y.ToString(nfi)}" .
                            {cIri} {ESProp.HasDirectionYIriPrefix} "{connector.Direction}" .
                    """;
        }).ToList();

        return $$"""
                {{RdfConst.AllPrefixes}}

                INSERT DATA {
                    GRAPH {{sub}} {
                        {{sub}} {{ESProp.HasEngSymKeyIriPrefix}} "{{createDto.Key ?? symbolId}}" .
                        {{sub}} {{ESProp.HasDescriptionIriPrefix}} "{{createDto.Description}}" .
                        {{sub}} {{ESProp.IsTypeIriPrefix}} <{{RdfConst.SymbolTypeIri}}> .
                        {{sub}} {{ESProp.HasDateCreatedIriPrefix}} "{{DateTimeOffset.UtcNow:O}}"^^xsd:dateTime .
                        {{sub}} {{ESProp.HasDateUpdatedIriPrefix}} "{{DateTimeOffset.MinValue:O}}"^^xsd:dateTime .
                        {{sub}} {{ESProp.HasGeometryIriPrefix}} "{{createDto.GeometryPath}}" .
                        {{sub}} {{ESProp.HasWidthIriPrefix}} "{{createDto.Width.ToString(nfi)}}" .
                        {{sub}} {{ESProp.HasHeightIriPrefix}} "{{createDto.Height.ToString(nfi)}}" .
                        {{sub}} {{ESProp.HasOwnerIriPrefix}} "{{createDto.Owner}}" .
                        {{sub}} {{ESProp.HasSourceFilenameIriPrefix}} "{{createDto.Filename}}" .
                {{string.Join(Environment.NewLine, connectorTurtle)}}
                    }
                }
                """;
    }


    public static string? UpdateEngineeringSymbolQuery(string id, EngineeringSymbolUpdateDto dto)
    {
        var triples = new Dictionary<string,string>();

        var symbolGraph = $"{RdfConst.IndividualPrefix}:{id}";
        
        if (dto.Key != null)
        {
            triples.Add(ESProp.HasEngSymKeyIriPrefix, dto.Key);
        }
        
        if (dto.Owner != null)
        {
            triples.Add(ESProp.HasOwnerIriPrefix, dto.Owner);
        }
        
        if (dto.Description != null)
        {
            triples.Add(ESProp.HasDescriptionIriPrefix, dto.Description);
        }

        if (triples.Count == 0)
        {
            return null;
        }
        
        // Update dateUpdates as well!
        triples.Add(ESProp.HasDateUpdatedIriPrefix, DateTimeOffset.UtcNow.ToString("O"));
        
        var deleteTriples = string.Join(Environment.NewLine, 
            triples.Select(kvp => $"""
                                                            {symbolGraph} {kvp.Key} ?o .
                                                        """));

        var insertTriples = string.Join(Environment.NewLine,
            triples.Select(kvp =>
            {
                var unit = kvp.Key switch
                {
                    ESProp.HasDateUpdatedIriPrefix => "^^xsd:dateTime",
                    _ => ""
                };
                
                return $"""
                            {symbolGraph} {kvp.Key} "{kvp.Value}"{unit} .
                        """;
            }));
        
        return $$"""
                {{RdfConst.XsdPrefix}}
                {{RdfConst.EngSymPrefix}}
                {{RdfConst.SymbolPrefix}}

                WITH {{symbolGraph}}
                DELETE {
                {{deleteTriples}}
                }
                INSERT { 
                {{insertTriples}}
                }
                WHERE { ?s ?p ?o }
                """;
    }
    
}