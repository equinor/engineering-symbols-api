using System.Globalization;
using System.Xml.Linq;

namespace EngineeringSymbols.Tools.SvgParser;

internal static class SvgCrawler
{
	internal static SvgParserContext ExtractDataAndTransformElement(XElement element, SvgParserContext ctx)
	{
		element.TransformAndExtract(ctx);
		
		return element.Elements()
			.Fold(ctx, (current, child) => 
				ExtractDataAndTransformElement(child, current));
	}
	
	private static void TransformAndExtract(this XElement element, SvgParserContext ctx)
	{
		switch (element.Name.LocalName)
		{
			case "svg":
				element.TransformSvgElement(ctx);
				break;
			case "g":
				element.TransformGElement(ctx);
				break;
			case "path":
				element.TransformPathElement(ctx);
				break;
			case "circle":
				element.TransformCircleElement(ctx);
				break;
		}
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="element"></param>
	/// <param name="ctx"></param>
	private static void TransformSvgElement(this XElement element, SvgParserContext ctx)
	{
		// Extract SVG height

		if (!double.TryParse(element.Attribute("height")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture,
			    out var heightParsed))
		{
			ctx.AddParseError(SvgParseCategory.Dimensions,"SVG 'height' is missing or invalid");
		}
		
		if (heightParsed % 24 != 0)
		{
			ctx.AddParseError(SvgParseCategory.Dimensions,$"SVG 'height' is not a multiple of 24");
		}

		ctx.ExtractedData.Height = heightParsed;

		// Extract SVG width
		if (!double.TryParse(element.Attribute("width")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture,
			    out var widthParsed))
		{
			ctx.AddParseError(SvgParseCategory.Dimensions,"SVG 'width' is missing or invalid");
		}

		if (widthParsed % 24 != 0)
		{
			ctx.AddParseError(SvgParseCategory.Dimensions,$"SVG 'width' is not a multiple of 24");
		}
		
		ctx.ExtractedData.Width  = widthParsed;

		// Check viewBox

		var viewBox = element.Attribute("viewBox")?.Value;

		if (viewBox == null)
		{
			ctx.AddParseError(SvgParseCategory.Dimensions,$"Could not find viewBox attribute in <svg> element: {element}");
		}
		else
		{
			var viewBoxElements = viewBox
				.Split(" ")
				.Map(el =>Try(() => double.Parse(el, NumberStyles.Any, CultureInfo.InvariantCulture)).Try().Map(res => res).IfFail(-1d))
				.ToList();

			if (viewBoxElements.Contains(-1d))
			{
				ctx.AddParseError(SvgParseCategory.Dimensions,$"Could not parse viewBox attribute value in <svg> element");
			} else if (viewBoxElements.Count != 4)
			{
				ctx.AddParseError(SvgParseCategory.Dimensions,$"Could not parse viewBox attribute value in <svg> element");
			}
			else if(viewBoxElements[0] != 0 || viewBoxElements[1] != 0 || viewBoxElements[2] != widthParsed ||
			        viewBoxElements[3] != heightParsed)
			{
				ctx.AddParseError(SvgParseCategory.Dimensions,$"viewBox attribute value error in <svg> element. Expected '0 0 {widthParsed} {heightParsed}', but got '{viewBoxElements[0]} {viewBoxElements[1]} {viewBoxElements[2]} {viewBoxElements[3]}.'");
			}
		}
		
		// Remove styling

		element.RemoveStyling();

		// Remove stroke attribute as the symbols only should be colored using fill
		element.SetAttributeValue("stroke", null);
		element.SetAttributeValue("fill", ctx.Options.FillColor);
	}

	private static void TransformGElement(this XElement element, SvgParserContext ctx)
	{
		element.RemoveStyling();

		if (element.IsAnnotationGroup(ctx))
			element.ProcessAnnotations(ctx);
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="element"></param>
	private static void TransformPathElement(this XElement element, SvgParserContext ctx)
	{
		var parentId = element.Parent?.Attribute("id")?.Value.ToLower();
		var id = element.Attribute("id")?.Value.ToLower();

		if (id == "symbol" || parentId == "symbol")
		{
			var pathData = element.Attribute("d")?.Value;
			if (pathData != null)
				ctx.ExtractedData.PathData.Add(pathData);
		}

		element.RemoveStyling();
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="element"></param>
	private static void TransformCircleElement(this XElement element, SvgParserContext ctx)
	{
		element.RemoveStyling();
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="annotationsElement"></param>
	/// <param name="data"></param>
	private static void ProcessAnnotations(this XElement annotationsElement, SvgParserContext ctx)
	{
		foreach (var xElement in annotationsElement.Elements())
		{
			if (!xElement.IsAnnotationElement())
				continue;

			var idData = xElement.GetIdAttributeData();

			if (idData == null || idData.Length < 2)
				continue;

			switch (idData[1].ToLower())
			{
				case "connector":
					xElement.ParseConnectorElement(ctx);
					xElement.SetAttributeValue("fill", ctx.Options.ConnectorFillColor);
					xElement.SetAttributeValue("r", "0.5");
					break;
				case "rotationpoint":
					xElement.ParseRotationPointElement(ctx);
					break;
			}

			xElement.RemoveTransform();
		}

		// Check for duplicate connector id's
		var idList = ctx.ExtractedData.Connectors.Select(c => c.Id);

		var duplicates = idList?
			.GroupBy(x => x)
			.Where(g => g.Count() > 1)
			.Select(y => y.Key)
			.ToList();

		if (duplicates is {Count: > 0})
			ctx.AddParseError(SvgParseCategory.Connector,$"Duplicate connector id's found: {string.Join(",", duplicates)}");
		
		if (ctx.Options.RemoveAnnotations)
			annotationsElement.Remove();
	}

	private static void ParseConnectorElement(this XElement element, SvgParserContext ctx)
	{
		var idData = element.GetIdAttributeData();
		var elementType = element.Name.LocalName;

		if (idData is not { Length: 4 } || idData[1].ToLower() != "connector" || elementType != "circle")
			return;

		if (int.TryParse(idData[3], NumberStyles.Any, CultureInfo.InvariantCulture,
			    out var direction))
		{
			if (direction is < 0 or > 360)
				ctx.AddParseError(SvgParseCategory.Connector,$"Invalid connector direction '{direction}'. Accepted value: 0 <= DIR <= 360. Element: {element}");
		}
		else
		{
			ctx.AddParseError(SvgParseCategory.Connector,$"Could not parse connector direction from element: {element}");
			direction = -1;
		}
		
		// Parse x and y
		var cx = element.Attributes().FirstOrDefault(a => a.Name.LocalName == "cx");

		if (!double.TryParse(cx?.Value, NumberStyles.Any, CultureInfo.InvariantCulture,
			    out var xParsed))
		{
			ctx.AddParseError(SvgParseCategory.Connector,$"Could not parse x-coordinate from: {element}");
			xParsed = -1d;
		}
		
		
		var cy = element.Attributes().FirstOrDefault(a => a.Name.LocalName == "cy");

		if (!double.TryParse(cy?.Value, NumberStyles.Any, CultureInfo.InvariantCulture,
			    out var yParsed))
		{
			ctx.AddParseError(SvgParseCategory.Connector,$"Could not parse y-coordinate from: {element}");
			yParsed = -1d;
		}
		
		var id = idData[2];

		if (string.IsNullOrEmpty(id))
		{
			ctx.AddParseError(SvgParseCategory.Connector,$"Could not parse connector Id from: {element}");
		}
		
		ctx.ExtractedData.Connectors.Add(new EngineeringSymbolConnector
		{
			Id = id,
			Direction = direction,
			RelativePosition = new Point { X = xParsed, Y = yParsed }
		});
	}

	private static void ParseRotationPointElement(this XElement element, SvgParserContext ctx)
	{
		// TODO
	}

	private static bool IsAnnotationElement(this XElement element)
	{
		var idData = element.GetIdAttributeData();
		return idData is { Length: >= 2 } && idData[0] == "annotation";
	}

	private static bool IsAnnotationGroup(this XElement element, SvgParserContext ctx)
	{
		return string.Equals(element.Attribute("id")?.Value, ctx.Options.AnnotationsElementId, StringComparison.CurrentCultureIgnoreCase);
	}

	private static string[]? GetIdAttributeData(this XElement element)
	{
		return element.Attribute("id")?.Value.Split("-");
	}

	private static void RemoveStyling(this XElement element)
	{
		var removeAttrs = new[] { "fill", "stroke", "style" };
		element.Attributes().Where(a => removeAttrs.Contains(a.Name.LocalName)).Remove();
	}

	private static void RemoveTransform(this XElement element)
	{
		var removeAttrs = new[] { "transform" };
		element.Attributes().Where(a => removeAttrs.Contains(a.Name.LocalName)).Remove();
	}

}