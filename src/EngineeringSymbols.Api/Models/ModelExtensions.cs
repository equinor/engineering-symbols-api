using EngineeringSymbols.Api.Entities;
using EngineeringSymbols.Tools.Models;

namespace EngineeringSymbols.Api.Models;

public static class ModelExtensions
{
    public static EngineeringSymbolCreateDto ToCreateDto(this EngineeringSymbolParsed sym, string userId, string? filename = null)
    {
        return new EngineeringSymbolCreateDto
        {
            Key = sym.Key,
            Filename = filename ?? sym.Filename ?? "<Unknown>",
            Owner = userId,
            Width = sym.Width,
            Height = sym.Height,
            GeometryString = sym.GeometryString,
            SvgString = sym.SvgString,
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
            Filename = symbol.Filename,
            Key = symbol.Key,
            DateTimeCreated = symbol.DateTimeCreated,
            DateTimeUpdated = symbol.DateTimeUpdated,
            Owner = symbol.Owner,
            Width = symbol.Width,
            Height = symbol.Height,
            GeometryString = symbol.GeometryString,
            SvgString = symbol.SvgString,
            Connectors = symbol.Connectors
        };
    }
    
    public static EngineeringSymbolResponseDto ToResponseDto(this EngineeringSymbol symbol)
    {
        return new EngineeringSymbolResponseDto
        {
            Id = symbol.Id,
            Key = symbol.Key,
            Width = symbol.Width,
            Height = symbol.Height,
            GeometryString = symbol.GeometryString,
            SvgString = symbol.SvgString,
            Connectors = symbol.Connectors
        };
    }
}