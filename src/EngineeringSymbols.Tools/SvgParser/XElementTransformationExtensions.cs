using System.Globalization;
using System.Xml.Linq;
using EngineeringSymbols.Tools.Models;

namespace EngineeringSymbols.Tools.SvgParser;

internal static class XElementTransformationExtensions
{
	internal static void ApplySvgTransformations(this XElement element, TransformationData data)
	{
		element.Transform(data);

		foreach (var child in element.Elements())
			child.ApplySvgTransformations(data);
	}


	private static void Transform(this XElement element, TransformationData data)
	{
		switch (element.Name.LocalName)
		{
			case "svg":
				element.TransformSvgElement(data);
				break;
			case "g":
				element.TransformGElement(data);
				break;
			case "path":
				element.TransformPathElement(data);
				break;
			case "circle":
				element.TransformCircleElement(data);
				break;

		}
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="element"></param>
	private static void TransformSvgElement(this XElement element, TransformationData data)
	{
		// Extract SVG width and height

		if (!double.TryParse(element.Attribute("height")?.Value, out var heightParsed))
			throw new Exception("Failed to parse SVG height");

		if (heightParsed % 24 != 0)
		{
			throw new Exception($"Height is not a factor of 24 in element: {element}");
		}

		data.SymbolData.Height = heightParsed;

		if (!double.TryParse(element.Attribute("width")?.Value, out var widthParsed))
			throw new Exception("Failed to parse SVG width");

		if (widthParsed % 24 != 0)
		{
			throw new Exception($"Width is not a factor of 24 in element: {element}");
		}

		data.SymbolData.Width = widthParsed;

		// Check viewBox

		var viewBox = element.Attribute("viewBox")?.Value;

		if (viewBox == null)
			throw new Exception($"Could not find viewBox attribute in <svg> element: {element}");

		double[] viewBoxElements;

		try
		{
			viewBoxElements = viewBox.Split(" ").Select(s => double.Parse(s)).ToArray();
		}
		catch (Exception e)
		{
			throw new Exception($"Could not parse viewBox attribute value in <svg> element: {element}");
		}

		if (viewBoxElements[0] != 0 || viewBoxElements[1] != 0 || viewBoxElements[2] != widthParsed ||
			viewBoxElements[3] != heightParsed)
		{
			throw new Exception($"viewBox attribute value error in <svg> element. Expected '0 0 {widthParsed} {heightParsed}', but got '{viewBoxElements[0]} {viewBoxElements[1]} {viewBoxElements[2]} {viewBoxElements[3]}'. Element: {element}");
		}

		// Remove styling

		element.RemoveStyling();

		// Remove stroke attribute as the symbols only should be colored using fill
		element.SetAttributeValue("stroke", null);
		element.SetAttributeValue("fill", data.Options.FillColor);
	}

	private static void TransformGElement(this XElement element, TransformationData data)
	{
		element.RemoveStyling();

		if (element.IsAnnotationGroup(data))
			element.ProcessAnnotations(data);
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="element"></param>
	private static void TransformPathElement(this XElement element, TransformationData data)
	{
		var parentId = element.Parent?.Attribute("id")?.Value.ToLower();
		var id = element.Attribute("id")?.Value.ToLower();

		if (id == "symbol" || parentId == "symbol")
		{
			var pathData = element.Attribute("d")?.Value;
			if (pathData != null)
				data.SymbolData.PathData.Add(pathData);
		}

		element.RemoveStyling();
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="element"></param>
	private static void TransformCircleElement(this XElement element, TransformationData data)
	{
		element.RemoveStyling();
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="annotationsElement"></param>
	/// <param name="data"></param>
	private static void ProcessAnnotations(this XElement annotationsElement, TransformationData data)
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
					xElement.ParseConnectorElement(data);
					xElement.SetAttributeValue("fill", data.Options.ConnectorFillColor);
					xElement.SetAttributeValue("r", "0.5");
					break;
				case "rotationpoint":
					xElement.ParseRotationPointElement(data);
					break;
			}

			xElement.RemoveTransform();
		}

		// Check for duplicate connector id's
		var idList = data.SymbolData.Connectors.Select(c => c.Id);

		var duplicates = idList?
			.GroupBy(x => x)
			.Where(g => g.Count() > 1)
			.Select(y => y.Key)
			.ToList();

		if (duplicates is { Count: > 0 })
			throw new ArithmeticException($"Duplicate connector id's found: {string.Join(",", duplicates)}");

		if (data.Options.RemoveAnnotations)
			annotationsElement.Remove();
	}

	private static void ParseConnectorElement(this XElement element, TransformationData data)
	{
		var idData = element.GetIdAttributeData();
		var elementType = element.Name.LocalName;

		if (idData is not { Length: 4 } || idData[1].ToLower() != "connector" || elementType != "circle")
			return;

		if (!int.TryParse(idData[3], out var direction))
			throw new ArithmeticException($"Could not parse connector direction from element: {element}");

		if (direction is < 0 or >= 360)
			throw new ArithmeticException($"Invalid connector direction '{direction}'. Accepted value: 0 <= DIR < 360. Element: {element} ");

		// Parse x and y
		var cx = element.Attributes().FirstOrDefault(a => a.Name.LocalName == "cx");

		if (!double.TryParse(cx?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var x))
			throw new ArithmeticException($"Could not parse x-coordinate from: {element}");

		var cy = element.Attributes().FirstOrDefault(a => a.Name.LocalName == "cy");

		if (!double.TryParse(cy?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var y))
			throw new ArithmeticException($"Could not parse y-coordinate from: {element}");

		var id = idData[2];

		if (string.IsNullOrEmpty(id))
			throw new ArithmeticException($"Could not parse connector Id from: {element}");

		data.SymbolData.Connectors.Add(new EngineeringSymbolConnector
		{
			Id = id,
			Direction = direction,
			RelativePosition = new Point { X = x, Y = y }
		});
	}

	private static void ParseRotationPointElement(this XElement element, TransformationData data)
	{
		// TODO
	}

	private static bool IsAnnotationElement(this XElement element)
	{
		var idData = element.GetIdAttributeData();
		return idData is { Length: >= 2 } && idData[0] == "annotation";
	}

	private static bool IsAnnotationGroup(this XElement element, TransformationData data)
	{
		return string.Equals(element.Attribute("id")?.Value, data.Options.AnnotationsElementId, StringComparison.CurrentCultureIgnoreCase);
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