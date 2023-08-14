using EngineeringSymbols.Tools.Entities;
using EngineeringSymbols.Tools.Models;

namespace EngineeringSymbols.Api.Models;

public static class ModelExtensions
{
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
            Owner = symbol.Owner,
            Width = symbol.Width,
            Height = symbol.Height,
            Geometry = symbol.Geometry,
            Connectors = symbol.Connectors
        };
    }
}