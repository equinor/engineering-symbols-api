using EngineeringSymbols.Tools.Entities;
using EngineeringSymbols.Tools.Models;

namespace EngineeringSymbols.Api.Models;

public static class ModelExtensions
{
    public static EngineeringSymbolCreateDto ToCreateDto(this EngineeringSymbolParsed sym, string userId, string? filename = null)
    {
        return new EngineeringSymbolCreateDto
        {
            Key = sym.Key,
            Description = sym.Description ?? "None",
            Filename = filename ?? sym.Filename ?? "<Unknown>",
            Owner = userId,
            Width = sym.Width,
            Height = sym.Height,
            GeometryPath = sym.GeometryString,
            Connectors = sym.Connectors.Select(ToEngineeringSymbolConnector).ToList()
        };
    }
    
    private static EngineeringSymbolConnector ToEngineeringSymbolConnector(this EngineeringSymbolConnectorParsed parsed)
    {
        return new EngineeringSymbolConnector
        {
            Id = parsed.Id,
            RelativePosition = parsed.RelativePosition,
            Direction = parsed.Direction
        };
    }
    
    public static EngineeringSymbolCompleteResponseDto ToCompleteResponseDto(this EngineeringSymbol symbol)
    {
        return new EngineeringSymbolCompleteResponseDto
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
            Geometry = symbol.GeometryPath,
            Connectors = symbol.Connectors
        };
    }
    
    public static EngineeringSymbolResponseDto ToResponseDto(this EngineeringSymbol symbol)
    {
        return new EngineeringSymbolResponseDto
        {
            Id = symbol.Id,
            Key = symbol.Key,
            Description = symbol.Description,
            DateTimeCreated = symbol.DateTimeCreated,
            DateTimeUpdated = symbol.DateTimeUpdated,
            Width = symbol.Width,
            Height = symbol.Height,
            Geometry = symbol.GeometryPath,
            Connectors = symbol.Connectors
        };
    }
}