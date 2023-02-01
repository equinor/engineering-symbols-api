using System.Xml.Linq;
using EngineeringSymbols.Tools.Models;

namespace EngineeringSymbols.Tools.SvgParser;

public static class SvgParser
{
	public static EngineeringSymbol FromString(string svgString, Action<SvgTransformOptions>? options = null)
	{
		throw new NotImplementedException();
	}

	public static EngineeringSymbol FromFile(string filepath, Action<SvgTransformOptions>? options = null)
	{
		var mergedOptions = new SvgTransformOptions();
		options?.Invoke(mergedOptions);

		XElement rootSvgElement;

		string filePath;

		try
		{
			filePath = Path.GetFullPath(filepath);
			rootSvgElement = XElement.Load(filePath);
		}
		catch (Exception e)
		{
			throw new Exception($"Failed to load SVG file");
		}

		if (rootSvgElement.Name.LocalName != "svg")
			throw new Exception("Top level SVG element not found");

		mergedOptions.SymbolId ??= Helpers.GetSymbolId(Path.GetFileName(filePath));

		return rootSvgElement.ToEngineeringSymbol(mergedOptions);
	}

	public static EngineeringSymbol ToEngineeringSymbol(this XElement xmlElement, SvgTransformOptions options)
	{
		var transformationData = new TransformationData
		{
			SymbolData = new SymbolData(),
			Options = options,
		};

		var rawSvgString = options.IncludeRawSvgString ? xmlElement.ToString() : null;

		xmlElement.ApplySvgTransformations(transformationData);

		return new EngineeringSymbol
		{
			Id = options.SymbolId!,
			SvgString = options.IncludeSvgString ? xmlElement.ToString() : null,
			SvgStringRaw = rawSvgString,
			GeometryString = string.Join("", transformationData.SymbolData.PathData),
			Width = transformationData.SymbolData.Width,
			Height = transformationData.SymbolData.Height,
			Connectors = transformationData.SymbolData.Connectors,
		};
	}


}
