using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using EngineeringSymbols.Tools.Constants;
using EngineeringSymbols.Tools.Entities;
using EngineeringSymbols.Tools.Models;

namespace EngineeringSymbols.Tools.RdfParser;

public static class JsonLdParser
{
    public static List<EngineeringSymbol> ParseEngineeringSymbols(string jsonLdString)
    {
        var symbolsJsonDom = JsonSerializer.Deserialize<JsonObject>(jsonLdString);

        if (symbolsJsonDom is null)
        {
            throw new ArgumentException("Failed to deserialize JSON LD string.");
        }
        
        var symbols = new List<EngineeringSymbol>();

        if (symbolsJsonDom.ContainsKey("@id") && symbolsJsonDom.ContainsKey("@graph"))
        {
            symbols.Add(JsonObjectToEngineeringSymbol(symbolsJsonDom));
        }
        else if (symbolsJsonDom.ContainsKey("@graph"))
        {
            if (symbolsJsonDom["@graph"] is not JsonArray symbolGraphs)
            {
                throw new Exception("");
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
        var connectors = new List<EngineeringSymbolConnector>();
        
        // The nodes are either connectors or a symbol
        foreach (var node in graph)
        {
            if (node is not JsonObject ob) continue;

            if (ob.TryGetPropertyValue("@type", out var typeNode) && typeNode != null)
            {
                var typeIri = typeNode.ToString();

                if (typeIri.StartsWith(RdfConst.EngSymOntologyPrefix + ":"))
                {
                    typeIri = RdfConst.EngSymOntologyIri + typeIri.Split(":").Last().Trim();
                }

                switch (typeIri)
                {
                    case RdfConst.SymbolTypeIri:
                        symbolParsed = ParseSymbolObject(ob);
                        break;
                    case RdfConst.ConnectorTypeIri:
                        connectors.Add(ParseConnectorObject(ob));
                        break;
                }
            }
        }

        if (symbolParsed == null)
        {
            throw new Exception("Failed to parse EngineeringSymbol from JSON LD graph");
        }

        symbolParsed.Connectors.AddRange(connectors);

        return symbolParsed;
    }
    
    public static EngineeringSymbol ParseSymbolObject(JsonObject obj)
    {
        if (!Enum.TryParse<EngineeringSymbolStatus>(GetStringValue(obj, ESProp.HasStatusIriPrefix), out var statusEnum))
        {
            throw new ArgumentException("Failed to parse EngineeringSymbolStatus");
        }
        
        return new EngineeringSymbol(
            Id: GetStringValue(obj, ESProp.HasEngSymIdIriPrefix),
            Key: GetStringValue(obj,  ESProp.HasEngSymKeyIriPrefix),
            Status: statusEnum,
            Description: GetStringValue(obj,  ESProp.HasDescriptionIriPrefix),
            DateTimeCreated: GetDateTimeOffsetValue(obj,  ESProp.HasDateCreatedIriPrefix),
            DateTimeUpdated: GetDateTimeOffsetValue(obj, ESProp.HasDateUpdatedIriPrefix),
            DateTimePublished: GetDateTimeOffsetValue(obj, ESProp.HasDatePublishedIriPrefix),
            Owner: GetStringValue(obj,  ESProp.HasOwnerIriPrefix),
            Geometry: GetStringValue(obj,  ESProp.HasGeometryIriPrefix),
            Width: GetDoubleValue(obj, ESProp.HasWidthIriPrefix),
            Height: GetDoubleValue(obj,  ESProp.HasHeightIriPrefix),
            Connectors: new List<EngineeringSymbolConnector>());
    }
    
    public static EngineeringSymbolConnector ParseConnectorObject(JsonObject obj)
    {
        return new EngineeringSymbolConnector(
            Id: GetStringValue(obj, ESProp.HasNameIriPrefix),
            RelativePosition: new Point 
            {
                X = GetDoubleValue(obj,  ESProp.HasPositionXIriPrefix),
                Y = GetDoubleValue(obj,  ESProp.HasPositionYIriPrefix)
            },
            Direction: GetIntValue(obj,  ESProp.HasDirectionIriPrefix)
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
}