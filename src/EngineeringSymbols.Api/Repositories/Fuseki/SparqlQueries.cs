using System.Globalization;
using System.Text;
using EngineeringSymbols.Tools.Constants;
using EngineeringSymbols.Tools.Entities;

namespace EngineeringSymbols.Api.Repositories.Fuseki;

public static class SparqlQueries
{
    public const string GetAllSymbolsQuery_VOID = $$"""
                    {{RdfConst.EngSymPrefix}}
                    SELECT ?symbolGraph ?key WHERE { 
                        GRAPH ?symbolGraph { ?s {{ESProp.HasEngSymKeyIriPrefix}} ?key } 
                    }
                    """;
    
    public const string GetAllSymbolsQuery_VOID1 = $$"""
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

                SELECT ?symbolGraph ?key ?numVersions ?height
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
    
    public static string GetAllSymbolsQuery(bool distinct = false)
    {
        var distinctSubQuery = distinct ? $$"""
                                    {
                                        SELECT ?key (MAX(?dc) AS ?dateCreated) (COUNT(?g) AS ?numVersions)
                                        WHERE {
                                            GRAPH ?g { 
                                                ?s2 {{ESProp.HasEngSymKeyIriPrefix}} ?key .
                                                ?s2 {{ESProp.HasDateCreatedIriPrefix}} ?dc .
                                            }
                                        }
                                        GROUP BY ?key
                                    }
                                """ : "";
        
        return $$"""
                {{RdfConst.XsdPrefix}}
                {{RdfConst.EngSymPrefix}}
                {{RdfConst.SymbolPrefix}}

                SELECT ?symbolGraph ?id ?key ?status ?filename ?numVersions ?dateCreated ?dateUpdated ?owner ?description ?geometry ?width ?height ?connector ?connectorName ?connectorDirection ?connectorPosX ?connectorPosY
                WHERE {
                    GRAPH ?symbolGraph {
                        ?s1 {{ESProp.HasEngSymIdIriPrefix}} ?id .
                        ?s1 {{ESProp.HasEngSymKeyIriPrefix}} ?key .
                        ?s1 {{ESProp.HasStatusIriPrefix}} ?status .
                        ?s1 {{ESProp.HasSourceFilenameIriPrefix}} ?filename .
                        ?s1 {{ESProp.HasDateCreatedIriPrefix}} ?dateCreated . 
                        ?s1 {{ESProp.HasDateUpdatedIriPrefix}} ?dateUpdated .
                        ?s1 {{ESProp.HasDescriptionIriPrefix}} ?description .
                        ?s1 {{ESProp.HasGeometryIriPrefix}} ?geometry .
                        ?s1 {{ESProp.HasOwnerIriPrefix}} ?owner .
                        ?s1 {{ESProp.HasWidthIriPrefix}} ?width .
                        ?s1 {{ESProp.HasHeightIriPrefix}} ?height .
                        ?s1 {{ESProp.HasConnectorIriPrefix}} ?connector .
                        ?connector {{ESProp.HasNameIriPrefix}} ?connectorName .
                        ?connector {{ESProp.HasDirectionIriPrefix}} ?connectorDirection .
                        ?connector {{ESProp.HasPositionXIriPrefix}} ?connectorPosX .
                        ?connector {{ESProp.HasPositionYIriPrefix}} ?connectorPosY .
                    }
                {{distinctSubQuery}}
                }
                """;
    }
    
    public static string GetAllSymbolsConstructQuery(bool distinct = false)
    {
        var distinctSubQuery = distinct ? $$"""
                                    {
                                        SELECT ?key (MAX(?dc) AS ?dateCreated) (COUNT(?g) AS ?numVersions)
                                        WHERE {
                                            GRAPH ?g { 
                                                ?s2 {{ESProp.HasEngSymKeyIriPrefix}} ?key .
                                                ?s2 {{ESProp.HasDateCreatedIriPrefix}} ?dc .
                                            }
                                        }
                                        GROUP BY ?key
                                    }
                                """ : "";
        
        return $$"""
                {{RdfConst.XsdPrefix}}
                {{RdfConst.EngSymPrefix}}
                {{RdfConst.SymbolPrefix}}

                CONSTRUCT {
                    GRAPH ?symbolGraph {
                        ?s ?p ?o .
                    }
                }

                WHERE {
                    GRAPH ?symbolGraph {
                        ?s ?p ?o .
                        ?ss {{ESProp.HasDateCreatedIriPrefix}} ?dateCreated .
                    }
                {{distinctSubQuery}}
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
                            {cIri} {ESProp.HasNameIriPrefix} "{connector.Id}"^^xsd:string .
                            {cIri} {ESProp.HasPositionXIriPrefix} "{connector.RelativePosition.X.ToString(nfi)}"^^xsd:decimal .
                            {cIri} {ESProp.HasPositionYIriPrefix} "{connector.RelativePosition.Y.ToString(nfi)}"^^xsd:decimal .
                            {cIri} {ESProp.HasDirectionIriPrefix} "{connector.Direction}"^^xsd:integer .
                    """;
        }).ToList();

        return $$"""
                {{RdfConst.AllPrefixes}}

                INSERT DATA {
                    GRAPH {{sub}} {
                        {{sub}} {{ESProp.HasEngSymIdIriPrefix}} "{{symbolId}}"^^xsd:string .
                        {{sub}} {{ESProp.HasEngSymKeyIriPrefix}} "{{createDto.Key ?? symbolId}}"^^xsd:string .
                        {{sub}} {{ESProp.HasStatusIriPrefix}} "{{EngineeringSymbolStatus.Draft}}"^^xsd:string .
                        {{sub}} {{ESProp.HasDescriptionIriPrefix}} "{{createDto.Description}}"^^xsd:string .
                        {{sub}} {{ESProp.IsTypeIriPrefix}} <{{RdfConst.SymbolTypeIri}}> .
                        {{sub}} {{ESProp.HasDateCreatedIriPrefix}} "{{DateTimeOffset.UtcNow:O}}"^^xsd:dateTime .
                        {{sub}} {{ESProp.HasDateUpdatedIriPrefix}} "{{DateTimeOffset.MinValue:O}}"^^xsd:dateTime .
                        {{sub}} {{ESProp.HasGeometryIriPrefix}} "{{createDto.GeometryPath}}"^^xsd:string .
                        {{sub}} {{ESProp.HasWidthIriPrefix}} "{{createDto.Width.ToString(nfi)}}"^^xsd:integer .
                        {{sub}} {{ESProp.HasHeightIriPrefix}} "{{createDto.Height.ToString(nfi)}}"^^xsd:integer .
                        {{sub}} {{ESProp.HasOwnerIriPrefix}} "{{createDto.Owner}}"^^xsd:string .
                        {{sub}} {{ESProp.HasSourceFilenameIriPrefix}} "{{createDto.Filename}}"^^xsd:string .
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