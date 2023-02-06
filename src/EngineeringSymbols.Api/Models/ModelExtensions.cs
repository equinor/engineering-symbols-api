using EngineeringSymbols.Tools.Models;

namespace EngineeringSymbols.Api.Models;

public static class ModelExtensions
{
    public static EngineeringSymbolDto ToDto(this EngineeringSymbol symbol) => new()
    {
        Id = symbol.Id,
        Width = symbol.Width,
        Height = symbol.Height,
        SvgStringRaw = symbol.SvgStringRaw,
        SvgString = symbol.SvgString,
        GeometryString = symbol.GeometryString,
        Connectors = symbol.Connectors.Select(c => c.ToDto()).ToList()
    };

    public static EngineeringSymbolConnectorDto ToDto(this EngineeringSymbolConnector connector) =>
        new()
        {
            Id = connector.Id,
            RelativePosition = connector.RelativePosition,
            Direction = connector.Direction
        };
}