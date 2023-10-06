using System.Globalization;
using EngineeringSymbols.Tools.Constants;
using EngineeringSymbols.Tools.Entities;
using VDS.RDF;
using VDS.RDF.Nodes;
using VDS.RDF.Parsing;
using VDS.RDF.Writing;
using EsOntology = EngineeringSymbols.Tools.Constants.Ontology;
using GraphFactory = VDS.RDF.Configuration.GraphFactory;
using StringWriter = System.IO.StringWriter;

namespace EngineeringSymbols.Tools.RdfParser;


public static class SymbolGraphHelper
{
    public static void AssertTriple(this Graph graph, INode subject, INode predicate, INode obj)
    {
        graph.Assert(new Triple(subject, predicate, obj));
    }

    public static StringNode CreateDecimalNode(double value)
    {
        return new StringNode(value.ToString(CultureInfo.InvariantCulture),
            new Uri(XmlSpecsHelper.XmlSchemaDataTypeDecimal));
    }
    
    public static StringNode CreateIntegerNode(int value)
    {
        return new StringNode(value.ToString(CultureInfo.InvariantCulture),
            new Uri(XmlSpecsHelper.XmlSchemaDataTypeInteger));
    }
    
    public static (Graph, IUriNode) GetSymbolGraphBase(string symbolIdGuid)
    {
        var g = new Graph();
        
        
        
        g.NamespaceMap.AddNamespace(EsOntology.EngSymPrefix, new Uri(EsOntology.EngSymOntologyIri));
        g.NamespaceMap.AddNamespace(EsOntology.IndividualPrefix, new Uri(EsOntology.SymbolIri));
        g.NamespaceMap.AddNamespace(EsOntology.MetadataEditorPrefix, new Uri(EsOntology.MetadataEditorIri));
        g.NamespaceMap.AddNamespace("rdf", new Uri("http://www.w3.org/1999/02/22-rdf-syntax-ns#"));
        g.NamespaceMap.AddNamespace("rdfs", new Uri("http://www.w3.org/2000/01/rdf-schema#"));
        g.NamespaceMap.AddNamespace("xsd", new Uri("http://www.w3.org/2001/XMLSchema#"));
        g.NamespaceMap.AddNamespace("pav", new Uri("http://purl.org/pav/"));
        g.NamespaceMap.AddNamespace("foaf", new Uri("http://xmlns.com/foaf/0.1/"));
        g.NamespaceMap.AddNamespace("dc", new Uri("http://purl.org/dc/terms/"));
        
        //g.BaseUri = new Uri(EsOntology.SymbolIri);
        
        // g.NamespaceMap.AddNamespace("", new Uri(""));
        // g.NamespaceMap.AddNamespace("", new Uri(""));
        // g.NamespaceMap.AddNamespace("", new Uri(""));
        // g.NamespaceMap.AddNamespace("", new Uri(""));

        //var s = new UriNode(new Uri(symbolIdGuid, UriKind.Relative));
        
        //var s = g.CreateUriNode(new Uri(symbolIdGuid, UriKind.Relative));
        
        var s = g.CreateUriNode(EsOntology.IndividualPrefix + ":" + symbolIdGuid);
        
        return (g, s);
    }
    
    public static Graph CreateSymbolGraph(EngineeringSymbol symbol)
    {
        var (g, s) = GetSymbolGraphBase(symbol.Id);
        
        // Type
        g.AssertTriple(s, g.CreateUriNode(EsProp.IsTypeQName), g.CreateUriNode(EsOntology.SymbolTypeQName));
        
        // Id
        g.AssertTriple(s, g.CreateUriNode(EsProp.IdQname), new StringNode(symbol.Id));
        
        // Identifier
        g.AssertTriple(s, g.CreateUriNode(EsProp.IdentifierQName), new StringNode(symbol.Identifier));
        
        // Status
        g.AssertTriple(s, g.CreateUriNode(EsProp.EditorStatusQName), new StringNode(symbol.Status.ToString()));
        
        // Version
        g.AssertTriple(s, g.CreateUriNode(EsProp.VersionQName), new StringNode(symbol.Version));
        
        // PreviousVersion
        if (symbol.PreviousVersion != null)
        {
            g.AssertTriple(s, g.CreateUriNode(EsProp.PreviousVersionQName), g.CreateUriNode(symbol.PreviousVersion));
        }
        
        // Label
        g.AssertTriple(s, g.CreateUriNode(EsProp.LabelQName), new StringNode(symbol.Label));
        
        // Description
        g.AssertTriple(s, g.CreateUriNode(EsProp.DescriptionQName), new StringNode(symbol.Description));
        
        // Sources
        symbol.Sources.ForEach(so 
            => g.AssertTriple(s, g.CreateUriNode(EsProp.SourceQName), new StringNode(so)));
        
        // Subjects
        symbol.Subjects.ForEach(su 
            => g.AssertTriple(s, g.CreateUriNode(EsProp.SubjectQName), new StringNode(su)));
        
        // DateTimeCreated
        g.AssertTriple(s, g.CreateUriNode(EsProp.DateCreatedQName), new DateTimeNode(symbol.DateTimeCreated));
        
        // DateTimeModified
        g.AssertTriple(s, g.CreateUriNode(EsProp.DateModifiedQName), new DateTimeNode(symbol.DateTimeModified));
        
        // DateTimeIssued
        g.AssertTriple(s, g.CreateUriNode(EsProp.DateIssuedQName), new DateTimeNode(symbol.DateTimeIssued));
        
        // Creators
        symbol.Creators.ForEach(c =>
        {
            var cr = g.CreateBlankNode("creator_" + Guid.NewGuid());
            
            g.AssertTriple(cr, g.CreateUriNode("foaf:name"), new StringNode(c.Name));
            g.AssertTriple(cr, g.CreateUriNode("foaf:mbox"), new StringNode(c.Email));

            g.AssertTriple(s, g.CreateUriNode(EsProp.CreatorQName), cr);
        });
        
        // Contributors
        symbol.Contributors.ForEach(c =>
        {
            var cr = g.CreateBlankNode("contributor_" + Guid.NewGuid());
            
            g.AssertTriple(cr, g.CreateUriNode("foaf:name"), new StringNode(c.Name));
            g.AssertTriple(cr, g.CreateUriNode("foaf:mbox"), new StringNode(c.Email));

            g.AssertTriple(s, g.CreateUriNode(EsProp.ContributorQName), cr);
        });
        
        // Shape
        var shapeNode = g.CreateBlankNode("symbolShape");
        g.AssertTriple(shapeNode, g.CreateUriNode(EsProp.IsTypeQName), g.CreateUriNode(EsOntology.ShapeTypeQName));
        
        symbol.Shape.Serializations.ForEach(serialization =>
            g.AssertTriple(shapeNode, g.CreateUriNode(EsProp.HasSerializationQName), new StringNode(serialization.Value, new Uri(EsOntology.ShapeSerializationTypeSvgPathUri, UriKind.Absolute)))
        );

        symbol.Shape.Depictions.ForEach(depiction =>
            g.AssertTriple(shapeNode, g.CreateUriNode("foaf:depiction"), new StringNode(depiction))
        );
        
        g.AssertTriple(s, g.CreateUriNode(EsProp.HasShapeQName), shapeNode);
        
        // Width
        g.AssertTriple(s, g.CreateUriNode(EsProp.WidthQName),  CreateIntegerNode(symbol.Width));
        
        // Height
        g.AssertTriple(s, g.CreateUriNode(EsProp.HeightQName), CreateIntegerNode(symbol.Height));

        // DrawColor
        if (symbol.DrawColor != null)
        {
            g.AssertTriple(s, g.CreateUriNode(EsProp.DrawColorQName), new StringNode(symbol.DrawColor));
        }
        
        // FillColor
        if (symbol.FillColor != null)
        {
            g.AssertTriple(s, g.CreateUriNode(EsProp.FillColorQName), new StringNode(symbol.FillColor));
        }
        
        // CenterOfRotation
        var corNode = g.CreateBlankNode("centerOfRotation");
        g.AssertTriple(corNode, g.CreateUriNode(EsProp.IsTypeQName), g.CreateUriNode(EsOntology.CenterOfRotationTypeQName));
        g.AssertTriple(corNode, g.CreateUriNode(EsProp.PositionXQName), CreateDecimalNode(symbol.CenterOfRotation.X));
        g.AssertTriple(corNode, g.CreateUriNode(EsProp.PositionYQName), CreateDecimalNode(symbol.CenterOfRotation.Y));
        g.AssertTriple(s, g.CreateUriNode(EsProp.HasCenterOfRotationQName), corNode);
        
        // ConnectionPoints
        symbol.ConnectionPoints.ForEach(c =>
        {
            var point = g.CreateBlankNode("connectionPoint_" + c.Identifier);
            
            g.AssertTriple(point, g.CreateUriNode(EsProp.IdentifierQName), new StringNode(c.Identifier));
            g.AssertTriple(point, g.CreateUriNode(EsProp.PositionXQName), CreateDecimalNode(c.Position.X));
            g.AssertTriple(point, g.CreateUriNode(EsProp.PositionYQName), CreateDecimalNode(c.Position.Y));
            g.AssertTriple(point, g.CreateUriNode(EsProp.ConnectorDirectionQName), CreateIntegerNode(c.Direction));
            
            g.AssertTriple(point, g.CreateUriNode(EsProp.IsTypeQName), g.CreateUriNode(EsOntology.ConnectorTypeQName));
            g.AssertTriple(s, g.CreateUriNode(EsProp.HasConnectionPointQName), point);
        });
        
        return g;
    }

    public static async Task<string> EngineeringSymbolToTurtleString(EngineeringSymbol symbol)
    {
        var graph = CreateSymbolGraph(symbol);
        
        var turtleWriter = new CompressingTurtleWriter(TurtleSyntax.W3C)
        {
            HighSpeedModePermitted = false
        };

        await using var stringWriter = new StringWriter();
        
        turtleWriter.Save(graph, stringWriter);
                
        // Get the Turtle-formatted string
        return stringWriter.ToString();
    }
}