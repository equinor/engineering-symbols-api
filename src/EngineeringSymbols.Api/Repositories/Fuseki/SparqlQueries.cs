using System.Globalization;
using System.Text;

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
        
        var svgStrBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(createDto.SvgString));

        return $$"""
                {{RdfConst.AllPrefixes}}

                INSERT DATA {
                    GRAPH {{sub}} {
                        {{sub}} {{ESProp.HasEngSymKeyIriPrefix}} "{{createDto.Key ?? symbolId}}" .
                        {{sub}} {{ESProp.IsTypeIriPrefix}} <{{RdfConst.SymbolTypeIri}}> .
                        {{sub}} {{ESProp.HasDateCreatedIriPrefix}} "{{DateTimeOffset.UtcNow:O}}" .
                        {{sub}} {{ESProp.HasDateUpdatedIriPrefix}} "{{DateTimeOffset.MinValue:O}}" .
                        {{sub}} {{ESProp.HasGeometryIriPrefix}} "{{createDto.GeometryString}}" .
                        {{sub}} {{ESProp.HasSvgBase64IriPrefix}} "{{svgStrBase64}}" .
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
            triples.Select(kvp => $"""
                                                            {symbolGraph} {kvp.Key} "{kvp.Value}" .
                                                        """));
        
        return $$"""
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