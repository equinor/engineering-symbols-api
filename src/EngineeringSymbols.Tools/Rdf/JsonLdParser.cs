using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using EngineeringSymbols.Tools.Constants;
using EngineeringSymbols.Tools.Entities;
using EngineeringSymbols.Tools.Models;

namespace EngineeringSymbols.Tools.Rdf;

/*public static class JsonLdParser
{
    public static List<EngineeringSymbol> ParseEngineeringSymbols(string jsonLdString)
    {
        var symbolsJsonDom = JsonSerializer.Deserialize<JsonObject>(jsonLdString);

        if (symbolsJsonDom is null)
        {
            throw new ArgumentException("Failed to deserialize JSON LD string.");
        }
        
        var symbols = new List<EngineeringSymbol>();

        // We have 2 cases
        //
        // 1: a single graph this will result in top level @id
        // 2: multiple graphs, this will result in top level @graph
        
        if (symbolsJsonDom.ContainsKey("@id") && symbolsJsonDom.ContainsKey("@graph"))
        {
            // We have a single symbol/graph
            symbols.Add(JsonObjectToEngineeringSymbol(symbolsJsonDom));
        }
        else if (symbolsJsonDom.ContainsKey("@graph"))
        {
            if (symbolsJsonDom["@graph"] is not JsonArray symbolGraphs)
            {
                throw new Exception("Expected '@graph' field to be of type JsonArray");
            }

            symbols.AddRange(
                from symbolGraph in symbolGraphs 
                where symbolGraph != null 
                select JsonObjectToEngineeringSymbol(symbolGraph));
        }

        return symbols;
    }
    
    public static EngineeringSymbol JsonObjectToEngineeringSymbol(JsonNode jsonNode)
    {
        var jsonObject = jsonNode.AsObject();
        
        if (jsonObject["@graph"] is not JsonArray graph)
        {
            throw new Exception("Expected '@graph' field to be of type JsonArray");
        }

        EngineeringSymbol? symbolParsed = null;
        var connectors = new List<ConnectionPoint>();
        
        // The nodes are either connectors or a symbol
        foreach (var node in graph)
        {
            if (node is not JsonObject ob) continue;

            if (ob.TryGetPropertyValue("@type", out var typeNode) && typeNode != null)
            {
                var typeIri = typeNode.ToString();

                if (typeIri.StartsWith(Ontology.EngSymPrefix + ":"))
                {
                    typeIri = Ontology.EngSymOntologyIri + typeIri.Split(":").Last().Trim();
                }

                switch (typeIri)
                {
                    case Ontology.SymbolTypeIri:
                        symbolParsed = ParseSymbolObject(ob);
                        break;
                    case Ontology.ConnectionPointTypeIri:
                        connectors.Add(ParseConnectorObject(ob));
                        break;
                }
            }
        }

        if (symbolParsed == null)
        {
            throw new Exception("Failed to parse EngineeringSymbol from JSON LD graph");
        }

        symbolParsed.ConnectionPoints.AddRange(connectors);

        return symbolParsed;
    }
    
    public static EngineeringSymbol ParseSymbolObject(JsonObject obj)
    {
        if (!Enum.TryParse<EngineeringSymbolStatus>(GetStringValue(obj, EsProp.EditorStatusQName), out var statusEnum))
        {
            throw new ArgumentException("Failed to parse EngineeringSymbolStatus");
        }

        return new EngineeringSymbol
        {
            ShouldSerializeAsPublicVersion = false,
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
            Creators = null,
            Contributors = null,
            Shape = null,
            Width = GetIntValue(obj, EsProp.WidthQName),
            Height = GetIntValue(obj, EsProp.HeightQName),
            DrawColor = null,
            FillColor = null,
            CenterOfRotation = null,
            ConnectionPoints = null
        };

        /*return new EngineeringSymbol(
            Id: GetStringValue(obj, EsProp.HasEngSymIdQName),
            Identifier: GetStringValue(obj,  EsProp.IdentifierQName),
            Status: statusEnum,
            Description: GetStringValue(obj,  EsProp.DescriptionQName),
            DateTimeCreated: GetDateTimeOffsetValue(obj,  EsProp.DateCreatedQName),
            DateTimeModified: GetDateTimeOffsetValue(obj, EsProp.DateModifiedQName),
            DateTimeIssued: GetDateTimeOffsetValue(obj, EsProp.DateIssuedQName),
            Creator: GetStringValue(obj,  EsProp.CreatorQName),
            Geometry: GetStringValue(obj,  EsProp.HasShapeQName),
            Width: GetDoubleValue(obj, EsProp.WidthQName),
            Height: GetDoubleValue(obj,  EsProp.HeightQName),
            ConnectionPoints: new List<ConnectionPoint>());#1#
    }
    
    public static ConnectionPoint ParseConnectorObject(JsonObject obj)
    {
        return new ConnectionPoint(
            Identifier: GetStringValue(obj, EsProp.HasNameIriPrefix),
            Position: new Point 
            {
                X = GetDoubleValue(obj,  EsProp.PositionXQName),
                Y = GetDoubleValue(obj,  EsProp.PositionYQName)
            },
            Direction: GetIntValue(obj,  EsProp.ConnectorDirectionQName)
        );
    }
    
    private static string GetValuePropString(JsonObject obj, string prop)
    {
        if (!obj.TryGetPropertyValue(prop, out var node) || node is null)
        {
            throw new ArgumentException($"Property '{prop}' not found on JsonObject: {obj}");
        }
        
        if (node is not JsonObject valueObj) return node.ToString();
        
        if (valueObj.TryGetPropertyValue("@value", out var val) && val is not null)
        {
            return val.ToString();
        }
        
        throw new ArgumentException($"Property '{prop}' not found on JsonObject: {obj}");
    }   
    
    
    public static string GetStringValue(JsonObject obj, string prop)
    {
        return GetValuePropString(obj, prop);
    }
    
    public static DateTimeOffset GetDateTimeOffsetValue(JsonObject obj, string prop)
    {
        var valueString = GetValuePropString(obj, prop);

        if (DateTimeOffset.TryParse(valueString, out var parsedDateTime))
        {
            return parsedDateTime;
        }
        
        throw new FormatException($"Unable to parse date value: {valueString}");
    }
    
    public static int GetIntValue(JsonObject obj, string prop)
    {
        var valueString = GetValuePropString(obj, prop);

        if (int.TryParse(valueString, null, out var parsedInt))
        {
            return parsedInt;
        }
        
        throw new FormatException($"Unable to parse int value: {valueString}");
    }
    
    public static double GetDoubleValue(JsonObject obj, string prop)
    {
        var valueString = GetValuePropString(obj, prop);

        if (double.TryParse(valueString, CultureInfo.InvariantCulture, out var parsedDouble))
        {
            return parsedDouble;
        }
        
        throw new FormatException($"Unable to parse double value: {valueString}");
    }
}*/