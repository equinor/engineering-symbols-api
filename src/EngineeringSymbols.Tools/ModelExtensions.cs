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
}