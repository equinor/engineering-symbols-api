namespace EngineeringSymbols.Tools.Constants;

public static class RdfConst
{
    public const string BaseIri = "https://rdf.equinor.com";
    
    public const string IndividualPrefix = "symbol";
    
    public const string SymbolTypeName = "Symbol";
    
    public const string ConnectorTypeName = "Connector";
    
    public const string EngSymOntologyPrefix = "es";

    // Predicate base IRIs

    public const string EngSymOntologyIri = BaseIri + "/ontology/engineering-symbol/v1#";
    
    public const string SymbolTypeIri = EngSymOntologyIri + SymbolTypeName;
    public const string ConnectorTypeIri = EngSymOntologyIri + ConnectorTypeName;
    
    public const string SymbolTypeIriPrefix = EngSymOntologyPrefix + ":" + SymbolTypeName;
    public const string ConnectorTypeIriPrefix = EngSymOntologyPrefix + ":" + ConnectorTypeName;
    
    public const string SymbolIri = BaseIri + "/engineering-symbols/";
    
    public const string RdfIri = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";
    public const string RdfsIri = "http://www.w3.org/2000/01/rdf-schema#";
    public const string XsdIri = "http://www.w3.org/2001/XMLSchema#";
    
    // Prefixes (turtle)
    
    public const string RdfPrefix = $"PREFIX rdf: <{RdfIri}>";
    public const string RdfsPrefix = $"PREFIX rdfs: <{RdfsIri}>";
    public const string XsdPrefix = $"PREFIX xsd: <{XsdIri}>";
    
    public const string EngSymPrefix = $"PREFIX {EngSymOntologyPrefix}: <{EngSymOntologyIri}>";
    public const string SymbolPrefix = $"PREFIX {IndividualPrefix}: <{SymbolIri}>";
    
    public const string AllPrefixes = $$"""
                                        {{RdfPrefix}}
                                        {{RdfsPrefix}}
                                        {{XsdPrefix}}
                                        {{EngSymPrefix}}
                                        {{SymbolPrefix}}
                                        """;
    
}