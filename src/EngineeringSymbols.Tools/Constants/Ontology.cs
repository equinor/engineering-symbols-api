namespace EngineeringSymbols.Tools.Constants;
/// <summary>
/// Engineering Symbol Ontology
/// </summary>
public static class Ontology
{
    public const string BaseIri = "https://rdf.equinor.com";
    
    public const string IndividualPrefix = "symbol";
    
    public const string SymbolTypeName = "Symbol";
    public const string ConnectorTypeName = "ConnectionPoint";
    public const string ShapeTypeName = "Shape";
    public const string CenterOfRotationTypeName = "CenterOfRotation";
    
    
    public const string EngSymPrefix = "sym";

    // Predicate base IRIs

    public const string EngSymOntologyIri = BaseIri + "/ontology/engineering-symbol/v1#";
    
    public const string SymbolTypeIri = EngSymOntologyIri + SymbolTypeName;
    public const string ConnectionPointTypeIri = EngSymOntologyIri + ConnectorTypeName;
    
    public const string SymbolTypeQName = EngSymPrefix + ":" + SymbolTypeName;
    public const string ConnectorTypeQName = EngSymPrefix + ":" + ConnectorTypeName;
    public const string ShapeTypeQName = EngSymPrefix + ":" + ShapeTypeName;
    public const string CenterOfRotationTypeQName = EngSymPrefix + ":" + CenterOfRotationTypeName;
    
    
    public const string SymbolIri = BaseIri + "/engineering-symbols/";
    
    public const string RdfIri = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";
    public const string RdfsIri = "http://www.w3.org/2000/01/rdf-schema#";
    public const string XsdIri = "http://www.w3.org/2001/XMLSchema#";
    public const string PavIri = "http://purl.org/pav/";
    public const string FoafIri = "http://xmlns.com/foaf/0.1/";
    public const string DcIri = "http://purl.org/dc/terms/";
    
    
    
    public const string MetadataEditorIri = BaseIri + "/engineering-symbol-metadata-editor#";
    public const string ShapeSerializationTypeSvgPathUri = $"{EngSymOntologyIri}svg-path-data";
    
    // Prefix defs (turtle)
    
    public const string RdfPrefixDef = $"PREFIX rdf: <{RdfIri}>";
    public const string RdfsPrefixDef = $"PREFIX rdfs: <{RdfsIri}>";
    public const string XsdPrefixDef = $"PREFIX xsd: <{XsdIri}>";
    public const string PavPrefixDef = $"PREFIX pav: <{PavIri}>";
    public const string FoafPrefixDef = $"PREFIX foaf: <{FoafIri}>";
    public const string DcPrefixDef = $"PREFIX dc: <{DcIri}>";

    public const string MetadataEditorPrefix = "esmde";
    public const string MetadataEditorPrefixDef = $"PREFIX {MetadataEditorPrefix}: <{MetadataEditorIri}>";
    
    public const string EngSymPrefixDef = $"PREFIX {EngSymPrefix}: <{EngSymOntologyIri}>";
    public const string SymbolPrefixDef = $"PREFIX {IndividualPrefix}: <{SymbolIri}>";
    
    public const string AllPrefixDefs = $$"""
                                        {{RdfPrefixDef}}
                                        {{RdfsPrefixDef}}
                                        {{XsdPrefixDef}}
                                        {{EngSymPrefixDef}}
                                        {{SymbolPrefixDef}}
                                        {{MetadataEditorPrefixDef}}
                                        {{PavPrefixDef}}
                                        {{FoafPrefixDef}}
                                        {{DcPrefixDef}}
                                        """;
}