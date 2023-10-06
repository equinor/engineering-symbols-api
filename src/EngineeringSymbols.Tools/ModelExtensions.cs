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
            Sources = dto.Sources ?? new List<string>(),
            Subjects = dto.Subjects ?? new List<string>(),
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
    
    public static EngineeringSymbolCreateDto ToCreateDto(this EngineeringSymbolSvgParsed sym)
    {
        return new EngineeringSymbolCreateDto
        {
            Identifier = sym.Key,
            Label = string.Empty,
            Description = string.Empty,
            Sources = new List<string>(),
            Subjects = new List<string>(),
            Creators = new List<User>(),
            Contributors = new List<User>(),
            Shape = new Shape(new List<ShapeSerialization>
            {
                new ()
                {
                    Type = ShapeSerializationType.SvgPathData,
                    Value = StringHelpers.RemoveAllWhitespaceExceptSingleSpace(sym.Geometry)
                }
            }, new List<string>()),
            Width = sym.Width,
            Height = sym.Height,
            DrawColor = null,
            FillColor = null,
            CenterOfRotation = new Point {X = sym.Width / 2, Y = sym.Height / 2},
            ConnectionPoints = new List<ConnectionPoint>()
        };
    }
}