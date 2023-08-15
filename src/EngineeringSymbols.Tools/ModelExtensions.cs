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
            dto.Filename, 
            dto.Geometry,
            dto.Width ?? default,
            dto.Height ?? default,
            dto.Connectors);
    }
    
    public static EngineeringSymbol ToEngineeringSymbol(this EngineeringSymbolDto dto)
    {
        var status = Enum.Parse<EngineeringSymbolStatus>(dto.Status);

        return new EngineeringSymbol(
            dto.Id, 
            dto.Key, 
            status, 
            dto.Description, 
            dto.DateTimeCreated.Value,
            dto.DateTimeUpdated.Value, 
            dto.DateTimePublished.Value, 
            dto.Owner, 
            dto.Filename, 
            dto.Geometry,
            dto.Width ?? default,
            dto.Height ?? default,
            dto.Connectors);
    }
    
    public static EngineeringSymbolCreateDto ToCreateDto(this EngineeringSymbolSvgParsed sym, string key, string userId, string description = "None", string? filename = null)
    {
        return new EngineeringSymbolCreateDto
        {
            Key = key,
            Description = description,
            Filename = filename ?? sym.Filename,
            Owner = userId,
            Width = sym.Width,
            Height = sym.Height,
            Geometry = sym.Geometry,
            Connectors = new List<EngineeringSymbolConnector>()
        };
    }
    
    public static EngineeringSymbolDto ToDto(this EngineeringSymbol symbol)
    {
        return new EngineeringSymbolDto
        {
            Id = symbol.Id,
            Key = symbol.Key,
            Status = symbol.Status.ToString(),
            Description = symbol.Description,
            Filename = symbol.Filename,
            DateTimeCreated = symbol.DateTimeCreated,
            DateTimeUpdated = symbol.DateTimeUpdated,
            DateTimePublished = symbol.DateTimePublished,
            Owner = symbol.Owner,
            Width = symbol.Width,
            Height = symbol.Height,
            Geometry = symbol.Geometry,
            Connectors = symbol.Connectors
        };
    }
    
    public static EngineeringSymbolDto ToDto(this EngineeringSymbolCreateDto symbol)
    {
        return new EngineeringSymbolDto
        {
            Id = string.Empty,
            Key = symbol.Key,
            Status = EngineeringSymbolStatus.Draft.ToString(),
            Description = symbol.Description,
            Filename = symbol.Filename,
            DateTimeCreated = DateTimeOffset.MaxValue,
            DateTimeUpdated = DateTimeOffset.MaxValue,
            DateTimePublished = DateTimeOffset.MaxValue,
            Owner = symbol.Owner,
            Width = symbol.Width,
            Height = symbol.Height,
            Geometry = symbol.Geometry,
            Connectors = symbol.Connectors
        };
    }
    
    public static EngineeringSymbolPublicDto ToPublicDto(this EngineeringSymbol symbol)
    {
        return new EngineeringSymbolPublicDto
        {
            Id = symbol.Id,
            Key = symbol.Key,
            Description = symbol.Description,
            DateTimePublished = symbol.DateTimePublished,
            Width = symbol.Width,
            Height = symbol.Height,
            Geometry = symbol.Geometry,
            Connectors = symbol.Connectors.Map(c => new EngineeringSymbolConnectorPublicDto
            {
                Id = c.Id,
                RelativePosition = c.RelativePosition,
                Direction = c.Direction
            }).ToList()
        };
    }
}