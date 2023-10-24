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

}