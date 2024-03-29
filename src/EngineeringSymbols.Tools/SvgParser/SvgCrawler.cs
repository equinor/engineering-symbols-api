using System.Globalization;
using System.Xml.Linq;
using EngineeringSymbols.Tools.Constants;

namespace EngineeringSymbols.Tools.SvgParser;

internal static class SvgCrawler
{
	internal static SvgParserContext ValidateAndExtractData(XElement element, SvgParserContext ctx)
	{
		element.ValidateAndExtractElementData(ctx);

		return element.Elements()
			.Fold(ctx, (current, child) =>
				ValidateAndExtractData(child, current));
	}

	private static void ValidateAndExtractElementData(this XElement element, SvgParserContext ctx)
	{
		switch (element.Name.LocalName)
		{
			case "svg":
				element.ProcessSvgElement(ctx);
				break;
			case "metadata":
				element.ProcessMetadataElement(ctx);
				break;
			case "path":
				element.ProcessPathElement(ctx);
				break;
		}
	}

	private static void ProcessSvgElement(this XElement element, SvgParserContext ctx)
	{
		if (!int.TryParse(element.Attribute("height")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture,
				out var heightParsed))
		{
			ctx.AddParseError(SvgParseCategory.Dimensions, "SVG 'height' is missing or invalid");
		}

		ctx.ExtractedData.Height = heightParsed;

		if (!int.TryParse(element.Attribute("width")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture,
				out var widthParsed))
		{
			ctx.AddParseError(SvgParseCategory.Dimensions, "SVG 'width' is missing or invalid");
		}

		ctx.ExtractedData.Width = widthParsed;

		// Check viewBox if defined (not required)
		if (element.Attribute("viewBox")?.Value is string viewBox)
		{
			var viewBoxElements = viewBox
				.Split(" ")
				.Map(el =>
					Try(() =>
						int.Parse(el, NumberStyles.Any, CultureInfo.InvariantCulture))
						.Try()
						.IfFail(-1))
				.ToList();

			if (viewBoxElements.Contains(-1))
			{
				ctx.AddParseError(SvgParseCategory.Dimensions, $"Could not parse viewBox attribute value in <svg> element");
			}
			else if (viewBoxElements.Count != 4)
			{
				ctx.AddParseError(SvgParseCategory.Dimensions, $"Could not parse viewBox attribute value in <svg> element");
			}
			else if (viewBoxElements[0] != 0 || viewBoxElements[1] != 0 || viewBoxElements[2] != widthParsed || viewBoxElements[3] != heightParsed)
			{
				var msgDetails = heightParsed != 0d && widthParsed != 0d
					? $"Expected '0 0 {widthParsed} {heightParsed}', but got '{viewBoxElements[0]} {viewBoxElements[1]} {viewBoxElements[2]} {viewBoxElements[3]}.'"
					: "";

				ctx.AddParseError(SvgParseCategory.Dimensions, $"viewBox attribute value error in <svg> element. " + msgDetails);
			}
		}
	}

	private static void ProcessMetadataElement(this XElement element, SvgParserContext ctx)
	{
		var metadata = element.Elements();

		foreach (var el in metadata)
		{
			// Assume that all versions has the "key" property
			if (el.Name.NamespaceName != Ontology.EngSymOntologyIri) continue;

			// Note: We do not extract the key/identifier from the SVG files anymore. But leaves this
			// code as an example of how to extract metadata.
			
			// switch (el.Name.LocalName)
			// {
			// 	case "key":
			// 		ctx.ExtractedData.Key = el.Value;
			// 		break;
			// }
		}
	}

	private static void ProcessPathElement(this XElement element, SvgParserContext ctx)
	{
		var transform = element.Attribute("transform")?.Value;

		if (transform != null)
		{
			ctx.AddParseError(SvgParseCategory.Geometry, $"'transform' attribute on <path> element is not allowed");
		}

		var fillRule = element.Attribute("fill-rule")?.Value;

		if (fillRule != null && fillRule != "nonzero")
		{
			ctx.AddParseError(SvgParseCategory.Geometry, $"'fill-rule' attribute value on <path> can only be 'nonzero'");
		}

		var pathData = element.Attribute("d")?.Value;

		if (pathData != null)
			ctx.ExtractedData.PathData.Add(pathData);
	}
}