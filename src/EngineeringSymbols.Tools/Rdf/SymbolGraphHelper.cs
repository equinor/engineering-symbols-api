using System.Globalization;
using EngineeringSymbols.Tools.Constants;
using EngineeringSymbols.Tools.Entities;
using EngineeringSymbols.Tools.Utils;
using Newtonsoft.Json.Linq;
using VDS.RDF;
using VDS.RDF.JsonLd;
using VDS.RDF.Nodes;
using VDS.RDF.Parsing;
using VDS.RDF.Writing;
using EsOntology = EngineeringSymbols.Tools.Constants.Ontology;
using GraphFactory = VDS.RDF.Configuration.GraphFactory;
using StringWriter = System.IO.StringWriter;

namespace EngineeringSymbols.Tools.Rdf;


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
        if (!string.IsNullOrEmpty(symbol.PreviousVersion))
        {
            g.AssertTriple(s, g.CreateUriNode(EsProp.PreviousVersionQName), g.CreateUriNode(new Uri(symbol.PreviousVersion)));
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
        
        // User oid (object id from token)
        g.AssertTriple(s, g.CreateUriNode(EsProp.UserObjectIdQName), new StringNode(symbol.UserIdentifier));
        
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
            CompressionLevel = WriterCompressionLevel.None,
            //HighSpeedModePermitted = false,
            //PrettyPrintMode = true
        };

        await using var stringWriter = new StringWriter();
        
        turtleWriter.Save(graph, stringWriter);
                
        // Get the Turtle-formatted string
        return stringWriter.ToString();
    }

    public static async Task<string> EngineeringSymbolToTurtleStrin(EngineeringSymbol symbol)
    {
        var frame = await FileHelpers.GetJsonLdFrame();
            
        var aa = JToken.Parse("stringContent");

        var bb = JToken.Parse(frame);

        var a = JsonLdProcessor.Frame(aa, bb, new JsonLdProcessorOptions());
        return "";
    }
    
    /// <summary>
    /// This method only parse values and do not validate
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static Try<List<EngineeringSymbol>> ToEngineeringSymbols(JObject obj)
    {
        return () =>
        {
            var symbols = new List<EngineeringSymbol>();
            
            if (obj.ContainsKey("@id"))
            {
                // We have a single symbol/graph
                symbols.Add(JObjectSymbolGraphToEngineeringSymbol(obj));
            }
            else if (obj.ContainsKey("@graph"))
            {
                 if (obj["@graph"] is not JArray symbolGraphs)
                 {
                     throw new Exception("Expected '@graph' field to be of type JsonArray");
                 }

                symbols.AddRange(
                    from symbolGraph in symbolGraphs 
                    where symbolGraph != null
                    select JObjectSymbolGraphToEngineeringSymbol(symbolGraph as JObject));
            }
            
            return symbols;
        };
    }

    public static EngineeringSymbol JObjectSymbolGraphToEngineeringSymbol(JObject graph)
    {
        /*{
  "@id": "https://rdf.equinor.com/engineering-symbols/4d193516-dbaf-4829-afd9-e5f327bc2dc6",
  "@type": "sym:Symbol",
  "dc:contributor": {
    "foaf:mbox": "arnm@equinor.com",
    "foaf:name": "Arne M."
  },
  "dc:created": "2023-11-10T09:28:24+01:00",
  "dc:creator": {
    "foaf:mbox": "loba@equinor.com",
    "foaf:name": "Lorentz F. Barstad"
  },
  "dc:description": "A chocolate pumping device!",
  "dc:identifier": "PP007A",
  "dc:issued": "1970-01-01T01:00:00+01:00",
  "dc:modified": "2023-10-13T10:43:45.56349+02:00",
  "pav:version": "1",
  "rdfs:label": "Pump PP007A",
  "esmde:id": "4d193516-dbaf-4829-afd9-e5f327bc2dc6",
  "esmde:oid": "d5327a96-0771-4c0c-9334-bd14a0d3cb09",
  "esmde:status": "Draft",
  "https://rdf.equinor.com/ontology/engineering-symbol/v1#drawColor": "black",
  "https://rdf.equinor.com/ontology/engineering-symbol/v1#fillColor": "#000",
  "sym:hasCenterOfRotation": {
    "@type": "sym:CenterOfRotation",
    "sym:positionX": "48",
    "sym:positionY": "40.5"
  },
  "sym:hasConnectionPoint": [
    {
      "@type": "sym:ConnectionPoint",
      "dc:identifier": "gNrwVKPlFQ",
      "sym:connectorDirection": "90",
      "sym:positionX": "87",
      "sym:positionY": "13.5"
    },
    {
      "@type": "sym:ConnectionPoint",
      "dc:identifier": "gNrwVKKSISD",
      "sym:connectorDirection": "180",
      "sym:positionX": "89.1",
      "sym:positionY": "10.5"
    },
    {
      "@type": "sym:ConnectionPoint",
      "dc:identifier": "jCGoSOYbuj",
      "sym:connectorDirection": "270",
      "sym:positionX": "3.1",
      "sym:positionY": "-5"
    }
  ],
  "sym:hasShape": [
    {
      "@type": "sym:Shape",
      "foaf:depiction": "https://www.italianfoodtech.com/files/2017/06/OMAC.jpg",
      "sym:hasSerialization": {
        "@value": "M52 40.5C52 42.7091 50.2091 44.5 48 44.5C45.7909 44.5 44 42.7091 44 40.5C44 38.2909 45.7909 36.5 48 36.5C50.2091 36.5 52 38.2909 52 40.5ZM51 40.5C51 38.8431 49.6569 37.5 48 37.5C46.3431 37.5 45 38.8431 45 40.5C45 42.1569 46.3431 43.5 48 43.5C49.6569 43.5 51 42.1569 51 40.5ZM88 20.5H76.7266C80.6809 26.1692 83 33.0638 83 40.5C83 50.9622 78.4096 60.3522 71.1329 66.7659L83 90.5H13L24.8671 66.7659C17.5904 60.3522 13 50.9622 13 40.5C13 21.17 28.67 5.5 48 5.5H88V20.5ZM48 74.5C66.7777 74.5 82 59.2777 82 40.5C82 33.0245 79.5874 26.1124 75.4984 20.5C75.2521 20.1619 74.9997 19.8285 74.7413 19.5H86.8571V6.5H49.1429V6.51885C48.7634 6.50631 48.3825 6.5 48 6.5C29.2223 6.5 14 21.7223 14 40.5C14 59.2777 29.2223 74.5 48 74.5ZM25.6501 67.4359L14.618 89.5H81.382L70.3499 67.4359C64.2874 72.4719 56.4973 75.5 48 75.5C39.5027 75.5 31.7126 72.4719 25.6501 67.4359Z",
        "@type": "sym:svg-path-data"
      }
    }
  ],
  "sym:height": "96",
  "sym:width": "96",
  "pav:previousVersion": null,
  "dc:source": null,
  "dc:subject": null,*/
        
        var id = graph.ContainsKey("");
        
        return new EngineeringSymbol
        {
            Id = null,
            Identifier = null,
            Status = EngineeringSymbolStatus.None,
            Version = null,
            PreviousVersion = null,
            Label = null,
            Description = null,
            Sources = null,
            Subjects = null,
            DateTimeCreated = default,
            DateTimeModified = default,
            DateTimeIssued = default,
            UserIdentifier = null,
            Creators = null,
            Contributors = null,
            Shape = null,
            Width = 0,
            Height = 0,
            DrawColor = null,
            FillColor = null,
            CenterOfRotation = null,
            ConnectionPoints = null
        };
    }
}