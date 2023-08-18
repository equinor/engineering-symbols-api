namespace EngineeringSymbols.Api.Models;

public static class ContentTypes
{
    // Text types
    public const string Plain = "text/plain";
    public const string Html = "text/html";
    public const string Css = "text/css";
    public const string JavaScript = "application/javascript";

    // Application types
    public const string Json = "application/json";
    public const string Xml = "application/xml";
    public const string OctetStream = "application/octet-stream";

    // Image types
    public const string Svg = "image/svg+xml";
    public const string Jpeg = "image/jpeg";
    public const string Png = "image/png";
    public const string Gif = "image/gif";

    // RDF types
    public const string RdfXml = "application/rdf+xml";          // RDF/XML format
    public const string Ntriples = "application/n-triples";       // N-Triples format
    public const string Turtle = "text/turtle";                   // Turtle format
    public const string N3 = "text/n3";                           // Notation3 (N3) format
    public const string JsonLd = "application/ld+json";           // JSON-LD format
    public const string RdfThrift = "application/rdf+thrift";     // RDF Thrift format
    public const string SparqlJson = "application/sparql-results+json";
}