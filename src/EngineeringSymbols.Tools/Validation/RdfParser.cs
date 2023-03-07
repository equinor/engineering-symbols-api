using System.Globalization;
using EngineeringSymbols.Tools.Constants;
using EngineeringSymbols.Tools.Entities;
using EngineeringSymbols.Tools.Models;
using VDS.RDF;
using VDS.RDF.Parsing;
using INode = VDS.RDF.INode;
using NodeType = VDS.RDF.NodeType;

namespace EngineeringSymbols.Api.Utils;

public static class RdfParser
{
    public static Validation<Error, EngineeringSymbol> TurtleToEngineeringSymbol(string turtle)
    {
        var graph = new Graph();

        try
        {
            StringParser.Parse(graph, turtle);
        }
        catch (Exception)
        {
            return Fail<Error, EngineeringSymbol>(Error.New("Failed to parse Turtle"));
        }

        // Get symbol subject Iri
        var symbolSubjectV = graph.GetSymbolSubjectNode();
        
        var sIri = symbolSubjectV.Map(node => node.Uri.AbsoluteUri).ToOption().IfNoneUnsafe(() => null);
        
        if(sIri == null)
            return Fail<Error, EngineeringSymbol>(Error.New("Symbol Subject node is missing"));
        
        var idV = ValidateId(sIri);
        
        //var labelV = graph.GetStringLiteral(sIri, ESProp.HasLabelIri);
        
        var keyV = graph.GetStringLiteral(sIri, ESProp.HasEngSymKeyIri);
        
        var dateCreatedV = graph.GetDateTimeOffsetLiteral(sIri, ESProp.HasDateCreatedIri);
        var dateUpdatedV = graph.GetDateTimeOffsetLiteral(sIri, ESProp.HasDateUpdatedIri);
        
        var ownerV = graph.GetStringLiteral(sIri, ESProp.HasOwnerIri);
        
        var filenameV = graph.GetStringLiteral(sIri, ESProp.HasSourceFilenameIri);
        
        var svgB64V = graph.GetStringLiteral(sIri, ESProp.HasSvgBase64Iri);
        
        var geometryV = graph.GetStringLiteral(sIri, ESProp.HasGeometryIri);
        
        var widthV = graph.GetDoubleLiteral(sIri, ESProp.HasWidthIri);
        var heightV = graph.GetDoubleLiteral(sIri, ESProp.HasHeightIri);
        
        var connectorsIris = graph.GetObjectNodes(sIri, ESProp.HasConnectorIri)
            .Map(nodes => nodes.Fold(new List<UriNode>(), (uriNodes, node) =>
                {
                    var uriNode = GetUriNode(node).IfNoneUnsafe(() => null);
                    if(uriNode != null)
                        uriNodes.Add(uriNode);
                    return uriNodes;
                }))
            .Match(nodes => nodes, _ => new List<UriNode>{})
            .Select(node => node.Uri.AbsoluteUri)
            .ToList();
        
        var connectorsV = graph.GetConnectors(connectorsIris);
        
        return (idV, keyV, dateCreatedV, dateUpdatedV, ownerV, filenameV, svgB64V, geometryV, widthV, heightV, connectorsV).Apply(
            (id, key, dateCreated, dateUpdated, owner, filename, svgB64, geometry, width, height, connectors) =>
                new EngineeringSymbol
                {
                    Id = id,
                    Key = key,
                    DateTimeCreated = dateCreated,
                    DateTimeUpdated = dateUpdated,
                    Owner = owner,
                    Filename = filename,
                    SvgString = svgB64,
                    GeometryString = geometry,
                    Width = width,
                    Height = height,
                    Connectors = connectors
                });
    }

    public static string? AbsUri(INode node) => (node as UriNode)?.Uri.AbsoluteUri;

    public static string? SubUri(Triple triple) => AbsUri(triple.Subject);
    public static string? PredUri(Triple triple) => AbsUri(triple.Predicate);
    public static string? ObjUri(Triple triple) => AbsUri(triple.Object);
    
    public static Validation<Error, UriNode> GetSymbolSubjectNode(this Graph graph)
    {
        var symbolNode = graph.Triples.FirstOrDefault(t =>
            PredUri(t) == ESProp.IsTypeIri
            && ObjUri(t) == RdfConst.SymbolTypeIri)?.Subject as UriNode;
        
        return symbolNode?.Uri.AbsoluteUri == null 
            ? Fail<Error, UriNode>(Error.New("Symbol subject node missing")) 
            : Success<Error, UriNode>(symbolNode);
    }
    
    public static Validation<Error, string> ValidateId(string symbolSubjectIri)
    {
        var id = symbolSubjectIri.Split("/").Last();

        return Guid.TryParse(id, out var idParsed)
            ? Success<Error, string>(idParsed.ToString())
            : Fail<Error, string>(Error.New("Invalid Symbol Id"));
    }
    
    public static Validation<Error, List<EngineeringSymbolConnector>> GetConnectors(this Graph graph, List<string> cIris)
    {
        var result = new List<EngineeringSymbolConnector> { };

        var err = new Seq<Error>();
        
        foreach (var cIri in cIris)
        {
            var idV = graph.GetStringLiteral(cIri, ESProp.HasNameIri);
            var posXV = graph.GetDoubleLiteral(cIri, ESProp.HasPositionXIri);
            var posYV = graph.GetDoubleLiteral(cIri, ESProp.HasPositionYIri);
            var dirV = graph.GetIntegerLiteral(cIri, ESProp.HasDirectionYIri);

            var connector = (idV, posXV, posYV, dirV)
                .Apply((id, x, y, dir) => new EngineeringSymbolConnector
                {
                    Id = id,
                    RelativePosition = new Point
                    {
                        X = x,
                        Y = y
                    },
                    Direction = dir
                });

            connector.Match(
                Succ: symbolConnector => result.Add(symbolConnector),
                Fail: seq => err = err.Concat(seq));
        }

        if (err.Count > 0)
            return Fail<Error, List<EngineeringSymbolConnector>>(err);
        
        return Success<Error, List<EngineeringSymbolConnector>>(result);
    }
    
    public static Validation<Error, INode> GetObjectNode(this Graph graph, string subject, string predicate)
    {
        var objectNode = graph.Triples
            .FirstOrDefault(t => SubUri(t) == subject && PredUri(t) == predicate)?.Object;

        return objectNode == null
            ? Fail<Error, INode>(new Error($"Triple for subject '{subject}' and predicate '{predicate}' not found")) 
            : Success<Error, INode>(objectNode);
    }
    
    public static Validation<Error, INode[]> GetObjectNodes(this Graph graph, string subject, string predicate)
    {
        var objectNodes = graph.Triples
            .Where(t => SubUri(t) == subject && PredUri(t) == predicate).Select(triple => triple.Object).ToArray();

        return objectNodes.Length == 0
            ? Fail<Error, INode[]>(new Error($"Triples for subject '{subject}' and predicate '{predicate}' not found")) 
            : Success<Error, INode[]>(objectNodes);
    }
    
    public static Validation<Error, T> GetObjectValue<T>(this Graph graph, string subject, string predicate, Func<INode, Option<T>> objectNodeParser)
    {
        return GetObjectNode(graph, subject, predicate)
            .Bind(node => objectNodeParser(node)
                .Map(Success<Error, T>)
                .IfNone(Fail<Error, T>(new Error($"Failed to parse {typeof(T)} from node {node}"))));
    }

    public static Validation<Error, double> GetDoubleLiteral(this Graph graph, string subject, string predicate)
    {
        static Option<double> DoubleParser(INode node)
        {
            return GetLiteralNode(node)
                .Match(
                    Some: literalNode => double.TryParse(literalNode.Value, CultureInfo.InvariantCulture, out var value) 
                        ? Some(value) 
                        : Option<double>.None, 
                    None: () => Option<double>.None);
        }
        
        return GetObjectValue(graph, subject, predicate, DoubleParser);
    }
    
    public static Option<UriNode> GetUriNode(INode node)
    {
        var uriNode = node as UriNode;
        return uriNode is not {NodeType: NodeType.Uri} 
            ? Option<UriNode>.None
            : Optional(uriNode);
    }
    
    public static Option<LiteralNode> GetLiteralNode(INode node)
    {
        var literalNode = node as LiteralNode;
        return literalNode is not {NodeType: NodeType.Literal} 
            ? Option<LiteralNode>.None
            : Optional(literalNode);
    }
    
    public static Validation<Error, int> GetIntegerLiteral(this Graph graph, string subject, string predicate)
    {
        static Option<int> IntParser(INode node)
        {
            return GetLiteralNode(node)
                .Match(
                    Some: literalNode => int.TryParse(literalNode.Value, out var value) 
                        ? Some(value) 
                        : Option<int>.None, 
                    None: () => Option<int>.None);
        }
        
        return GetObjectValue(graph, subject, predicate, IntParser);
    }
    
    public static Validation<Error, string> GetStringLiteral(this Graph graph, string subject, string predicate)
    {
        return GetObjectValue(graph, subject, predicate, 
            node => GetLiteralNode(node).Map(literalNode => literalNode.Value));
    }
    
    public static Validation<Error, DateTimeOffset> GetDateTimeOffsetLiteral(this Graph graph, string subject, string predicate)
    {
        return GetObjectValue(graph, subject, predicate, 
            node => GetLiteralNode(node).Bind(l => parseDateTimeOffset(l.Value)));
    }
    

    
    public class Error : NewType<Error, string>
    {
        public Error(string e) : base(e)
        {
        }
    }
}