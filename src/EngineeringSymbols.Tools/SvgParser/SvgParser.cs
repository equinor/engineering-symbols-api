﻿using System.Xml.Linq;

namespace EngineeringSymbols.Tools.SvgParser;

public static class SvgParser
{
	public static Result<SvgParserResult> FromString(string svgString, Action<SvgParserOptions>? options = null)
	{
		return ParseSvgFlow(LoadRootXElementFromString(svgString), options);
	}

	public static Result<SvgParserResult> FromFile(string filepath, Action<SvgParserOptions>? options = null)
	{
		return ParseSvgFlow(LoadRootXElementFromFile(filepath), options);
	}
	
	private static Result<SvgParserResult> ParseSvgFlow(Func<SvgParserContext, Try<SvgParserContext>> xElementLoader, Action<SvgParserOptions>? options = null)
	{
		return CreateSvgParserContext(options)
			.Bind(xElementLoader)
			.Bind(ValidateRootElement)
			.Bind(ParseSvgData)
			.Map(ctx => ctx.ToSvgParserResult())
			.Try();
	}
	
	private static Try<SvgParserContext> CreateSvgParserContext(Action<SvgParserOptions>? options = null)
	{
		return () =>
		{
			var mergedOptions = new SvgParserOptions();
			options?.Invoke(mergedOptions);
			return new SvgParserContext { Options = mergedOptions};
		};
	}
	
	private static Func<SvgParserContext, Try<SvgParserContext>> LoadRootXElementFromString(string svgString)
	{
		return (SvgParserContext ctx) => () => 
			Try(() => XElement.Parse(svgString))
				.Map(el => 
				{ 
					ctx.SvgRootElement = el; 
					return ctx;
					
				})
				.IfFail(_ => throw new SvgParseException("Failed to load SVG from string"));
		
	}
	
	private static Func<SvgParserContext, Try<SvgParserContext>> LoadRootXElementFromFile(string filePath)
	{
		return (SvgParserContext ctx) => () =>
			filePath.Apply(path => Try(() => Path.GetFullPath(path)))
				.Bind(fullPath => Try(() => XElement.Load(fullPath)))
				.Map(el =>
				{
					ctx.SvgRootElement = el;
					return ctx;
				})
				.IfFail(e => throw new SvgParseException($"Failed to load SVG from file: {e.Message}"));
	}
	
	private static Try<SvgParserContext> ValidateRootElement(SvgParserContext ctx)
	{
		return () =>
		{
			if (ctx.SvgRootElement == null  || ctx.SvgRootElement.Name.LocalName != "svg")
				throw new SvgParseException("Invalid SVG content");
			
			return ctx;
		};
	}
	
	private static Try<SvgParserContext> ParseSvgData(SvgParserContext ctx)
	{
		return () =>
		{
			//ctx.ExtractedData.Id = ctx.Options.SymbolId ?? Helpers.GetSymbolId(Path.GetFileName(filePath));
			
			// Store input SVG before transforming it
			if (ctx.Options.IncludeRawSvgString)
				ctx.ExtractedData.RawSvgInputString = ctx.SvgRootElement.ToString();
			
			return SvgCrawler.ExtractDataAndTransformElement(ctx.SvgRootElement, ctx);
		};
	}
}