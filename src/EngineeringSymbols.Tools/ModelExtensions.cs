using EngineeringSymbols.Tools.Entities;
using EngineeringSymbols.Tools.Utils;

namespace EngineeringSymbols.Tools;

public static class ModelExtensions
{
	public static EngineeringSymbol ToInsertEntity(this EngineeringSymbolPutDto dto)
	{
		return new EngineeringSymbol
		{
			Id = Guid.NewGuid().ToString(),
			Status = EngineeringSymbolStatus.Draft,
			Identifier = dto.Identifier,
			Version = "SET_WHEN_ISSUED",
			PreviousVersion = null,
			Label = dto.Label,
			Description = dto.Description,
			Sources = dto.Sources ?? new List<string>(),
			Subjects = dto.Subjects ?? new List<string>(),
			DateTimeCreated = DateTime.UtcNow,
			DateTimeModified = DateTime.UnixEpoch,
			DateTimeIssued = DateTime.UnixEpoch,
			UserIdentifier = dto.UserIdentifier,
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

	public static EngineeringSymbolPutDto ToPutDto(this EngineeringSymbolSvgParsed sym)
	{
		return new EngineeringSymbolPutDto
		{
			Identifier = StringHelpers.GetRandomIdentifier(),
			Label = string.Empty,
			Description = string.Empty,
			Sources = new List<string>(),
			Subjects = new List<string>(),
			UserIdentifier = string.Empty,
			Creators = new List<User>(),
			Contributors = new List<User>(),
			Shape = new Shape
			{
				Serializations = new List<ShapeSerialization>
			{
				new ()
				{
					Type = ShapeSerializationType.SvgPathData,
					Value = StringHelpers.RemoveAllWhitespaceExceptSingleSpace(sym.Geometry)
				}
			},
				Depictions = new List<string>()
			},
			Width = sym.Width,
			Height = sym.Height,
			DrawColor = null,
			FillColor = null,
			CenterOfRotation = new Point { X = sym.Width / 2, Y = sym.Height / 2 },
			ConnectionPoints = new List<ConnectionPoint>()
		};
	}
}