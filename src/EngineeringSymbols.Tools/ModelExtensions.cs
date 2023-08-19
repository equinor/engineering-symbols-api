using System.Globalization;
using EngineeringSymbols.Tools.Constants;
using EngineeringSymbols.Tools.Entities;

namespace EngineeringSymbols.Tools;

public static class ModelExtensions
{
    public static EngineeringSymbol ToEngineeringSymbol(this EngineeringSymbolCreateDto dto, string id)
    {
        return new EngineeringSymbol(
            id, 
            dto.Key, 
            EngineeringSymbolStatus.None, 
            dto.Description, 
            DateTimeOffset.Now,
            DateTimeOffset.MinValue, 
            DateTimeOffset.MinValue, 
            dto.Owner,
            dto.Geometry,
            dto.Width,
            dto.Height,
            dto.Connectors.Map(connectorDto 
                    => new EngineeringSymbolConnector(
                        connectorDto.Id,
                        connectorDto.RelativePosition,
                        connectorDto.Direction))
                .ToList());
    }
    
    public static EngineeringSymbol ToEngineeringSymbol(this EngineeringSymbolDto dto)
    {
        var status = Enum.Parse<EngineeringSymbolStatus>(dto.Status);

        return new EngineeringSymbol(
            dto.Id, 
            dto.Key, 
            status, 
            dto.Description, 
            dto.DateTimeCreated,
            dto.DateTimeUpdated, 
            dto.DateTimePublished, 
            dto.Owner,
            dto.Geometry,
            dto.Width,
            dto.Height,
            dto.Connectors.Map(connectorDto 
                => new EngineeringSymbolConnector(
                    connectorDto.Id,
                    connectorDto.RelativePosition,
                    connectorDto.Direction))
                .ToList()
            );
    }
    
    public static EngineeringSymbolCreateDto ToCreateDto(this EngineeringSymbolSvgParsed sym, string userId, string description = "None")
    {
        return new EngineeringSymbolCreateDto
        (
            Key: sym.Key,
            Description: description,
            Owner: userId,
            Geometry: sym.Geometry,
            Width: sym.Width,
            Height: sym.Height,
            Connectors: new List<EngineeringSymbolConnectorDto>());
    }
    
    public static EngineeringSymbolDto ToDto(this EngineeringSymbol symbol)
    {
        return new EngineeringSymbolDto(
            Id: symbol.Id,
            Key: symbol.Key,
            Status: symbol.Status.ToString(),
            Description: symbol.Description,
            DateTimeCreated: symbol.DateTimeCreated,
            DateTimeUpdated: symbol.DateTimeUpdated,
            DateTimePublished: symbol.DateTimePublished,
            Owner: symbol.Owner,
            Geometry: symbol.Geometry,
            Width: symbol.Width,
            Height: symbol.Height,
            Connectors: symbol.Connectors.Map(connector => 
                    new EngineeringSymbolConnectorDto(connector.Id, connector.RelativePosition, connector.Direction))
                .ToList()
        );
    }
    
    public static EngineeringSymbolDto ToDto(this EngineeringSymbolCreateDto symbol)
    {
        return new EngineeringSymbolDto(
            Id: string.Empty,
            Key: symbol.Key,
            Status: EngineeringSymbolStatus.Draft.ToString(),
            Description: symbol.Description,
            DateTimeCreated: DateTimeOffset.MinValue,
            DateTimeUpdated: DateTimeOffset.MinValue,
            DateTimePublished: DateTimeOffset.MinValue,
            Owner: symbol.Owner,
            Geometry: symbol.Geometry,
            Width: symbol.Width,
            Height: symbol.Height,
            Connectors: symbol.Connectors.Map(connector => 
                new EngineeringSymbolConnectorDto(connector.Id, connector.RelativePosition, connector.Direction))
                .ToList());
    }
    
    public static EngineeringSymbolPublicDto ToPublicDto(this EngineeringSymbol symbol)
    {
        return new EngineeringSymbolPublicDto(
            Id: symbol.Id,
            Key: symbol.Key,
            Description: symbol.Description,
            DateTimePublished: symbol.DateTimePublished,
            Geometry: symbol.Geometry,
            Width: symbol.Width,
            Height: symbol.Height,
            Connectors: symbol.Connectors.Map(connector => new EngineeringSymbolConnectorPublicDto(
                connector.Id,
                connector.RelativePosition,
                connector.Direction)).ToList());
    }

    public static string GetConnectorIriPrefix(string symbolId, string name)
    {
        return RdfConst.IndividualPrefix + ":" + symbolId + "_C_" + name;
    }
    
    public static string ToTurtle(this EngineeringSymbolDto symbol)
    {
        var connectorPr = symbol.Connectors.Select(connector => $"<{GetConnectorIriPrefix(symbol.Id, connector.Id)}>").ToList();
        var nfi = new NumberFormatInfo {NumberDecimalSeparator = "."};
        
        var hasConnectors = connectorPr.Count > 0
            ? $$"""
                    {{ESProp.HasConnectorIriPrefix}} {{string.Join(", ", connectorPr)}} .
                """
            : "";

        var end = symbol.Connectors.Count > 0 ? ";" : ".";
        
        var connectors = symbol.Connectors.Select(connector => 
            $$"""
            <{{GetConnectorIriPrefix(symbol.Id, connector.Id)}}>
                a {{RdfConst.ConnectorTypeIriPrefix}} ;
                {{ESProp.HasNameIriPrefix}} "{{connector.Id}}"^^xsd:string ;
                {{ESProp.HasDirectionIriPrefix}} "{{connector.Direction}}"^^xsd:integer ;
                {{ESProp.HasPositionXIriPrefix}} "{{connector.RelativePosition.X.ToString(nfi)}}"^^xsd:decimal ;
                {{ESProp.HasPositionYIriPrefix}} "{{connector.RelativePosition.Y.ToString(nfi)}}"^^xsd:decimal .
            """).ToList();
        
        return $$"""
                  {{RdfConst.AllPrefixes}}
                  <{{RdfConst.IndividualPrefix}}:{{symbol.Id}}>
                  
                    a {{RdfConst.SymbolTypeIriPrefix}} ;
                    {{ESProp.HasEngSymIdIriPrefix}} "{{symbol.Id}}"^^xsd:string ;
                    {{ESProp.HasEngSymKeyIriPrefix}} "{{symbol.Key}}"^^xsd:string ;
                    {{ESProp.HasStatusIriPrefix}} "{{symbol.Status}}"^^xsd:string ;
                    {{ESProp.HasDateCreatedIriPrefix}} "{{symbol.DateTimeCreated}}"^^xsd:dateTime ;
                    {{ESProp.HasDateUpdatedIriPrefix}} "{{symbol.DateTimeUpdated}}"^^xsd:dateTime ;
                    {{ESProp.HasDatePublishedIriPrefix}} "{{symbol.DateTimePublished}}"^^xsd:dateTime ;
                    {{ESProp.HasDescriptionIriPrefix}} "{{symbol.Description}}"^^xsd:string ;
                    {{ESProp.HasOwnerIriPrefix}} "{{symbol.Owner}}"^^xsd:string ;
                    {{ESProp.HasGeometryIriPrefix}} "{{symbol.Geometry}}"^^xsd:string ;
                    {{ESProp.HasWidthIriPrefix}} "{{symbol.Width}}"^^xsd:integer ;
                    {{ESProp.HasHeightIriPrefix}} "{{symbol.Height}}"^^xsd:integer {{end}}
                  {{hasConnectors}}
                  
                  {{string.Join(Environment.NewLine + Environment.NewLine, connectors)}}
                  """;
    }
}