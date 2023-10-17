using System.Globalization;
using EngineeringSymbols.Tools.Constants;
using EngineeringSymbols.Tools.Entities;
using EngineeringSymbols.Tools.Models;

namespace EngineeringSymbols.Api.Repositories.Fuseki;

public static class SparqlQueries
{
	public static string GetAllSymbolsConstructQuery(bool onlyLatestVersion = false, bool onlyIssued = true)
	{
		var onlyIssuedConstraint = onlyIssued
			? $"""
                    ?s2 {EsProp.EditorStatusQName} "{EngineeringSymbolStatus.Issued}" .
            """
			: "";

		var onlyLatestVersionSubQuery = onlyLatestVersion
			? $$"""
                  {
                      SELECT ?identifier (MAX(?dc) AS ?dateCreated) (COUNT(?g) AS ?numVersions)
                      WHERE {
                          GRAPH ?g {
                              ?s2 {{EsProp.IdentifierQName}} ?identifier .
                              ?s2 {{EsProp.DateCreatedQName}} ?dc .
                              {{onlyIssuedConstraint}}
                          }
                      }
                      GROUP BY ?identifier
                  }
              """
			: "";

		return $$"""
                {{Ontology.AllPrefixDefs}}

                CONSTRUCT {
                    GRAPH ?symbolGraph {
                        ?s ?p ?o .
                    }
                }

                WHERE {
                    GRAPH ?symbolGraph {
                        ?s ?p ?o .
                        ?s1 {{EsProp.DateCreatedQName}} ?dateCreated .
                {{onlyIssuedConstraint}}
                    }
                {{onlyLatestVersionSubQuery}}
                }
                """;
	}


	public static string SymbolExistByIdQuery(string id)
	{
		return $$"""
                {{Ontology.SymbolPrefixDef}}
                ASK WHERE { 
                    GRAPH {{Ontology.IndividualPrefix}}:{{id}} { ?s ?p ?o }
                }
                """;
	}

	public static string SymbolExistByKeyQuery(string key)
	{
		return $$"""
                {{Ontology.EngSymPrefixDef}}
                ASK WHERE { 
                    GRAPH ?g { 
                        ?s {{EsProp.IdentifierQName}} "{{key}}" 
                    }
                }
                """;
	}

	public static string GetEngineeringSymbolByIdQuery(string id, bool onlyIssued = true)
	{
		var onlyIssuedConstraint = onlyIssued
			? $"""
                    ?s2 {EsProp.EditorStatusQName} "{EngineeringSymbolStatus.Issued}" .
            """
			: "";

		return $$"""
                {{Ontology.AllPrefixDefs}}

                CONSTRUCT 
                {   
                    GRAPH ?symbolGraph {
                        ?s ?p ?o .
                    }
                }
                WHERE
                {
                    GRAPH ?symbolGraph {
                {{onlyIssuedConstraint}}
                        ?ss {{EsProp.IdQname}} "{{id}}" .
                        ?s ?p ?o .
                    }
                }
                """;
	}

	public static string GetEngineeringSymbolByIdentifierQuery(string identifier, bool onlyIssued = true)
	{
		var onlyIssuedConstraint = onlyIssued
			? $"""
                    ?s2 {EsProp.EditorStatusQName} "{EngineeringSymbolStatus.Issued}" .
            """
			: "";

		return $$"""
                {{Ontology.AllPrefixDefs}}

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
                {{onlyIssuedConstraint}}
                        ?ss {{EsProp.IdentifierQName}} "{{identifier}}" .
                        ?s ?o ?p .
                    }
                }
                """;
	}

	public static string DeleteEngineeringSymbolByIdQuery(string id)
	{
		return $"""
                {Ontology.SymbolPrefixDef}
                DROP GRAPH {Ontology.IndividualPrefix}:{id}
                """;
	}

	public static string UpdateEngineeringSymbolStatusQuery(string id, string status)
	{
		var symbolGraph = $"{Ontology.IndividualPrefix}:{id}";

		var dt = DateTime.UtcNow.ToString("O");
		
		var issuedTripleDelete = status == EngineeringSymbolStatus.Issued.ToString()
			? $"""
			   {symbolGraph} {EsProp.DateIssuedQName} ?o .
			   """
			: "";
		
		var issuedTripleInsert = status == EngineeringSymbolStatus.Issued.ToString()
			? $"""
			   {symbolGraph} {EsProp.DateIssuedQName} "{dt}"^^xsd:dateTime .
			   """
			: "";
        
		return $$"""
		         {{Ontology.XsdPrefixDef}}
		         {{Ontology.EngSymPrefixDef}}
		         {{Ontology.SymbolPrefixDef}}
		         {{Ontology.MetadataEditorPrefixDef}}
		         {{Ontology.DcPrefixDef}}

		         WITH {{symbolGraph}}
		         DELETE {
		             {{symbolGraph}} {{EsProp.EditorStatusQName}} ?o .
		             {{issuedTripleDelete}}
		         }
		         INSERT {
		             {{symbolGraph}} {{EsProp.EditorStatusQName}} "{{status}}"^^xsd:string .
		             {{issuedTripleInsert}}
		         }
		         WHERE { ?s ?p ?o }
		         """;
	}
	
	/*public static string InsertEngineeringSymbolQuery(EngineeringSymbol symbol)
	{
		var nfi = new NumberFormatInfo { NumberDecimalSeparator = "." };
		
		var sub = $"{Ontology.IndividualPrefix}:{symbol.Id}";
        
		var connectorTurtle = symbol.ConnectionPoints.Map(connector =>
		{
			var cIri = $"_:ConnectionPoint_{connector.Identifier}";

			return $"""
                            {sub} {EsProp.HasConnectionPointQName} {cIri} .
                            {cIri} {EsProp.IsTypeQName} <{Ontology.ConnectionPointTypeIri}> .
                            {cIri} {EsProp.IdentifierQName} "{connector.Identifier}"^^xsd:string .
                            {cIri} {EsProp.PositionXQName} "{connector.Position.X.ToString(nfi)}"^^xsd:decimal .
                            {cIri} {EsProp.PositionYQName} "{connector.Position.Y.ToString(nfi)}"^^xsd:decimal .
                            {cIri} {EsProp.ConnectorDirectionQName} "{connector.Direction}"^^xsd:integer .
                    """;
		}).ToList();
        
		
		return $$"""
                {{Ontology.AllPrefixDefs}}

                INSERT DATA {
                    GRAPH {{sub}} {
                        {{sub}} {{EsProp.IsTypeQName}} <{{Ontology.SymbolTypeIri}}> .
                        {{sub}} {{EsProp.HasEngSymIdQName}} "{{symbol.Id}}"^^xsd:string .
                        {{sub}} {{EsProp.IdentifierQName}} "{{symbol.Identifier}}"^^xsd:string .
                        {{sub}} {{EsProp.EditorStatusQName}} "{{EngineeringSymbolStatus.Draft}}"^^xsd:string .
                        {{sub}} {{EsProp.DescriptionQName}} "{{symbol.Description}}"^^xsd:string .
                        {{sub}} {{EsProp.DateCreatedQName}} "{{DateTimeOffset.UtcNow:O}}"^^xsd:dateTime .
                        {{sub}} {{EsProp.DateModifiedQName}} "{{DateTimeOffset.UnixEpoch:O}}"^^xsd:dateTime .
                        {{sub}} {{EsProp.DateIssuedQName}} "{{DateTimeOffset.UnixEpoch:O}}"^^xsd:dateTime .
                    
                        {{sub}} {{EsProp.WidthQName}} "{{symbol.Width.ToString(nfi)}}"^^xsd:integer .
                        {{sub}} {{EsProp.HeightQName}} "{{symbol.Height.ToString(nfi)}}"^^xsd:integer .
                  
                {{string.Join(Environment.NewLine, connectorTurtle)}}
                    }
                }
                """;
	}*/

	/*public static string? UpdateEngineeringSymbolQuery(string id, EngineeringSymbol symbol)
	{
		var triples = new Dictionary<string, string>();

		var symbolGraph = $"{Ontology.IndividualPrefix}:{id}";

		if (symbol.Identifier != null)
		{
			triples.Add(EsProp.IdentifierQName, symbol.Identifier);
		}

		if (symbol.Owner != null)
		{
			triples.Add(EsProp.CreatorQName, symbol.Owner);
		}

		if (symbol.Description != null)
		{
			triples.Add(EsProp.DescriptionQName, symbol.Description);
		}

		if (triples.Count == 0)
		{
			return null;
		}

		// Update dateUpdates as well!
		triples.Add(EsProp.DateModifiedQName, DateTimeOffset.UtcNow.ToString("O"));

		var deleteTriples = string.Join(Environment.NewLine,
			triples.Select(kvp => $"""
                                                            {symbolGraph} {kvp.Key} ?o .
                                                        """));

		var insertTriples = string.Join(Environment.NewLine,
			triples.Select(kvp =>
			{
				var unit = kvp.Key switch
				{
					EsProp.DateModifiedQName => "^^xsd:dateTime",
					_ => ""
				};

				return $"""
                            {symbolGraph} {kvp.Key} "{kvp.Value}"{unit} .
                        """;
			}));

		return $$"""
                {{Ontology.XsdPrefixDef}}
                {{Ontology.EngSymPrefixDef}}
                {{Ontology.SymbolPrefixDef}}

                WITH {{symbolGraph}}
                DELETE {
                {{deleteTriples}}
                }
                INSERT { 
                {{insertTriples}}
                }
                WHERE { ?s ?p ?o }
                """;
	}*/

}