using System.Globalization;
using System.Text;
using EngineeringSymbols.Tools.Constants;
using EngineeringSymbols.Tools.Entities;
using EngineeringSymbols.Tools.Utils;

namespace EngineeringSymbols.Tools;

public static class ModelExtensions
{
    public static EngineeringSymbol ToInsertEntity(this EngineeringSymbolCreateDto dto)
    {
        return new EngineeringSymbol
        {
            ShouldSerializeAsPublicVersion = false,
            
            Id = Guid.NewGuid().ToString(),
            Status = EngineeringSymbolStatus.Draft,
            Identifier = dto.Identifier,
            Version = "1",
            PreviousVersion = null,
            Label = dto.Label,
            Description = dto.Description,
            Sources = dto.Sources,
            Subjects = dto.Subjects,
            DateTimeCreated = DateTimeOffset.Now,
            DateTimeModified = DateTimeOffset.UnixEpoch,
            DateTimeIssued = DateTimeOffset.UnixEpoch,
            Creators = dto.Creators,
            Contributors = dto.Contributors,
            Shape = dto.Shape,
            Width = dto.Width,
            Height = dto.Height,
            DrawColor = dto.DrawColor,
            FillColor = dto.FillColor,
            CenterOfRotation = dto.CenterOfRotation,
            ConnectionPoints = dto.ConnectionPoints
        };
    }
    
    /*public static EngineeringSymbol ToInsertEntity(this EngineeringSymbolCreateDto dto)
    {
        return new EngineeringSymbol(
            Id: Guid.NewGuid().ToString(), 
            Identifier: dto.Key, 
            Status: EngineeringSymbolStatus.Draft, 
            Description: dto.Description, 
            DateTimeCreated: DateTimeOffset.Now,
            DateTimeModified: DateTimeOffset.UnixEpoch, 
            DateTimeIssued: DateTimeOffset.UnixEpoch, 
            Creator: dto.Owner,
            Geometry: StringHelpers.RemoveAllWhitespaceExceptSingleSpace(dto.Geometry),
            Width: dto.Width,
            Height: dto.Height,
            ConnectionPoints: dto.Connectors.Map(connectorDto 
                    => new ConnectionPoint(
                        connectorDto.Id,
                        connectorDto.RelativePosition,
                        connectorDto.Direction))
                .ToList());
    }*/
    
    public static EngineeringSymbol ToEngineeringSymbol(this EngineeringSymbolDto dto)
    {
        var status = Enum.Parse<EngineeringSymbolStatus>(dto.Status);

        return new EngineeringSymbol(
            Id: dto.Id, 
            Identifier: dto.Key, 
            Status: status, 
            Description: dto.Description, 
            DateTimeCreated: dto.DateTimeCreated,
            DateTimeModified: dto.DateTimeUpdated, 
            DateTimeIssued: dto.DateTimePublished, 
            Creator: dto.Owner,
            Geometry: dto.Geometry,
            Width: dto.Width,
            Height: dto.Height,
            ConnectionPoints: dto.Connectors.Map(connectorDto 
                => new ConnectionPoint(
                    connectorDto.Id,
                    connectorDto.RelativePosition,
                    connectorDto.Direction))
                .ToList()
            );
    }
    
    public static EngineeringSymbolCreateDto ToCreateDto(this EngineeringSymbolSvgParsed sym, string creatorEmail, string description = "None")
    {
        return new EngineeringSymbolCreateDto
        {
            Identifier = sym.Key,
            Label = string.Empty,
            Description = description,
            Sources = new List<string>(),
            Subjects = new List<string>(),
            Creators = new List<User>{ new User("",creatorEmail) },
            Contributors = new List<User>(),
            Shape = new Shape(new List<ShapeSerialization>
            {
                new (ShapeSerializationType.SvgPathData, StringHelpers.RemoveAllWhitespaceExceptSingleSpace(sym.Geometry))
            }, new List<string>()),
            Width = sym.Width,
            Height = sym.Height,
            DrawColor = null,
            FillColor = null,
            CenterOfRotation = new Point {X = sym.Width / 2, Y = sym.Height / 2},
            ConnectionPoints = new List<ConnectionPoint>()
        };
    }
    
    
    
    public static EngineeringSymbolDto ToDto(this EngineeringSymbolCreateDto symbol)
    {
        return new EngineeringSymbolDto(
            Id: string.Empty,
            Key: symbol.Key,
            Status: EngineeringSymbolStatus.None.ToString(),
            Description: symbol.Description,
            DateTimeCreated: DateTimeOffset.UnixEpoch,
            DateTimeUpdated: DateTimeOffset.UnixEpoch,
            DateTimePublished: DateTimeOffset.UnixEpoch,
            Owner: symbol.Owner,
            Geometry: symbol.Geometry,
            Width: symbol.Width,
            Height: symbol.Height,
            Connectors: symbol.Connectors.Map(connector => 
                new EngineeringSymbolConnectorDto(
                    Id: connector.Id,
                    RelativePosition: connector.RelativePosition,
                    Direction: connector.Direction))
                .ToList());
    }
    
    public static EngineeringSymbolPublicDto ToPublicDto(this EngineeringSymbol symbol)
    {
        return new EngineeringSymbolPublicDto(
            Id: symbol.Id,
            Key: symbol.Identifier,
            Description: symbol.Description,
            DateTimePublished: symbol.DateTimeIssued,
            Geometry: symbol.Geometry,
            Width: symbol.Width,
            Height: symbol.Height,
            Connectors: symbol.ConnectionPoints.Map(connector => new EngineeringSymbolConnectorPublicDto(
                Id: connector.Identifier,
                RelativePosition: connector.Position,
                Direction: connector.Direction)).ToList());
    }

    public static string GetConnectorIriPrefix(string symbolId, string name)
    {
        return Ontology.IndividualPrefix + ":" + symbolId + "_C_" + name;
    }
    
    public static string ToTurtle(this EngineeringSymbolDto symbol)
    {
        var connectorPr = symbol.Connectors.Select(connector => $"<{GetConnectorIriPrefix(symbol.Id, connector.Id)}>").ToList();
        var nfi = new NumberFormatInfo {NumberDecimalSeparator = "."};
        
        var hasConnectors = connectorPr.Count > 0
            ? $"    {EsProp.HasConnectionPointQName} {string.Join(", ", connectorPr)} ."
            : "";

        var end = symbol.Connectors.Count > 0 ? ";" : ".";
        
        var connectors = symbol.Connectors.Select(connector => 
            $"""
            <{GetConnectorIriPrefix(symbol.Id, connector.Id)}>
                a {Ontology.ConnectorTypeIriPrefix} ;
                {EsProp.HasNameIriPrefix} "{connector.Id}"^^xsd:string ;
                {EsProp.ConnectorDirectionQName} "{connector.Direction}"^^xsd:integer ;
                {EsProp.PositionXQName} "{connector.RelativePosition.X.ToString(nfi)}"^^xsd:decimal ;
                {EsProp.PositionYQName} "{connector.RelativePosition.Y.ToString(nfi)}"^^xsd:decimal .
            """).ToList();
        
        return $"""
                  {Ontology.AllPrefixDefs}
                  <{Ontology.IndividualPrefix}:{symbol.Id}>
                    a {Ontology.SymbolTypeIriPrefix} ;
                    {EsProp.HasEngSymIdQName} "{symbol.Id}"^^xsd:string ;
                    {EsProp.IdentifierQName} "{symbol.Key}"^^xsd:string ;
                    {EsProp.EditorStatusQName} "{symbol.Status}"^^xsd:string ;
                    {EsProp.DateCreatedQName} "{symbol.DateTimeCreated:O}"^^xsd:dateTime ;
                    {EsProp.DateModifiedQName} "{symbol.DateTimeUpdated:O}"^^xsd:dateTime ;
                    {EsProp.DateIssuedQName} "{symbol.DateTimePublished:O}"^^xsd:dateTime ;
                    {EsProp.DescriptionQName} "{symbol.Description}"^^xsd:string ;
                    {EsProp.CreatorQName} "{symbol.Owner}"^^xsd:string ;
                    {EsProp.HasShapeQName} "{symbol.Geometry}"^^xsd:string ;
                    {EsProp.WidthQName} "{symbol.Width}"^^xsd:integer ;
                    {EsProp.HeightQName} "{symbol.Height}"^^xsd:integer {end}
                  {hasConnectors}
                  
                  {string.Join(Environment.NewLine + Environment.NewLine, connectors)}
                  """;
    }
}