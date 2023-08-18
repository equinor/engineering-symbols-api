using System.Globalization;
using EngineeringSymbols.Tools.Constants;
using EngineeringSymbols.Tools.Entities;
using EngineeringSymbols.Tools.Models;

namespace EngineeringSymbols.Api.Repositories;

public static class SparqlQueries
{
    public static string GetAllSymbolsConstructQuery(bool distinct = false, bool onlyPublished = true)
    {
        var onlyPublishedConstraint = onlyPublished
            ? $$"""
                    ?s2 {{ESProp.HasStatusIriPrefix}} "Published" .
            """
            : "";
        
        var distinctSubQuery = distinct ? $$"""
                                    {
                                        SELECT ?key (MAX(?dc) AS ?dateCreated) (COUNT(?g) AS ?numVersions)
                                        WHERE {
                                            GRAPH ?g { 
                                                ?s2 {{ESProp.HasEngSymKeyIriPrefix}} ?key .
                                                ?s2 {{ESProp.HasDateCreatedIriPrefix}} ?dc .
                                                {{onlyPublishedConstraint}}
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
                        ?s1 {{ESProp.HasDateCreatedIriPrefix}} ?dateCreated .
                {{onlyPublishedConstraint}}
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
    
    public static string GetEngineeringSymbolByIdQuery(string id, bool onlyPublished = true)
    {
        var onlyPublishedConstraint = onlyPublished
            ? $$"""
                    ?s2 {{ESProp.HasStatusIriPrefix}} "Published" .
            """
            : "";
        
        return $$"""
                {{RdfConst.XsdPrefix}}
                {{RdfConst.RdfsPrefix}}
                {{RdfConst.SymbolPrefix}}
                {{RdfConst.EngSymPrefix}}

                CONSTRUCT 
                {   
                    GRAPH ?symbolGraph {
                        ?s ?p ?o .
                    }
                }
                WHERE
                {
                    GRAPH ?symbolGraph {
                {{onlyPublishedConstraint}}
                        ?ss {{ESProp.HasEngSymIdIriPrefix}} "{{id}}" .
                        ?s ?p ?o .
                    }
                }
                """;
    }

    public static string GetEngineeringSymbolByKeyQuery(string key, bool onlyPublished = true)
    {
        var onlyPublishedConstraint = onlyPublished
            ? $$"""
                    ?s2 {{ESProp.HasStatusIriPrefix}} "Published" .
            """
            : "";
        
        return $$"""
                {{RdfConst.XsdPrefix}}
                {{RdfConst.RdfsPrefix}}
                {{RdfConst.SymbolPrefix}}
                {{RdfConst.EngSymPrefix}}

                CONSTRUCT 
                {
                    GRAPH ?g {
                        ?s ?o ?p 
                    }
                }
                WHERE 
                {
                    GRAPH ?g 
                    {
                {{onlyPublishedConstraint}}
                        ?ss {{ESProp.HasEngSymKeyIriPrefix}} "{{key}}" .
                        ?s ?o ?p .
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
    
    public static string InsertEngineeringSymbolQuery(EngineeringSymbol symbol)
    {
        var nfi = new NumberFormatInfo {NumberDecimalSeparator = "."};
        var sub = $"{RdfConst.IndividualPrefix}:{symbol.Id}";
        
        var connectorTurtle = symbol.Connectors.Map(connector =>
        {
            var cIri = $"{RdfConst.IndividualPrefix}:{symbol.Id}_C_{connector.Id}";
                
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
                        {{sub}} {{ESProp.HasEngSymIdIriPrefix}} "{{symbol.Id}}"^^xsd:string .
                        {{sub}} {{ESProp.HasEngSymKeyIriPrefix}} "{{symbol.Key}}"^^xsd:string .
                        {{sub}} {{ESProp.HasStatusIriPrefix}} "{{EngineeringSymbolStatus.Draft}}"^^xsd:string .
                        {{sub}} {{ESProp.HasDescriptionIriPrefix}} "{{symbol.Description}}"^^xsd:string .
                        {{sub}} {{ESProp.IsTypeIriPrefix}} <{{RdfConst.SymbolTypeIri}}> .
                        {{sub}} {{ESProp.HasDateCreatedIriPrefix}} "{{DateTimeOffset.UtcNow:O}}"^^xsd:dateTime .
                        {{sub}} {{ESProp.HasDateUpdatedIriPrefix}} "{{DateTimeOffset.MinValue:O}}"^^xsd:dateTime .
                        {{sub}} {{ESProp.HasDatePublishedIriPrefix}} "{{DateTimeOffset.MinValue:O}}"^^xsd:dateTime .
                        {{sub}} {{ESProp.HasGeometryIriPrefix}} "{{symbol.Geometry}}"^^xsd:string .
                        {{sub}} {{ESProp.HasWidthIriPrefix}} "{{symbol.Width.ToString(nfi)}}"^^xsd:integer .
                        {{sub}} {{ESProp.HasHeightIriPrefix}} "{{symbol.Height.ToString(nfi)}}"^^xsd:integer .
                        {{sub}} {{ESProp.HasOwnerIriPrefix}} "{{symbol.Owner}}"^^xsd:string .
                {{string.Join(Environment.NewLine, connectorTurtle)}}
                    }
                }
                """;
    }

    public static string? UpdateEngineeringSymbolQuery(string id, EngineeringSymbolDto dto)
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