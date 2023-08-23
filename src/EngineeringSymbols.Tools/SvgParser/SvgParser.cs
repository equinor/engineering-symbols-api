using System.Xml.Linq;

namespace EngineeringSymbols.Tools.SvgParser;

public static class SvgParser
{
	public static Result<SvgParserResult> FromString(string svgString)
	{
		return ParseSvgFlow(LoadRootXElementFromString(svgString));
	}

	public static Result<SvgParserResult> FromFile(string filepath)
	{
		return ParseSvgFlow(LoadRootXElementFromFile(filepath));
	}
	
	private static Result<SvgParserResult> ParseSvgFlow(Func<SvgParserContext, Try<SvgParserContext>> xElementLoader)
	{
		return CreateSvgParserContext()
			.Bind(xElementLoader)
			.Bind(ValidateRootElement)
			.Bind(ParseSvgData)
			.Map(ctx => ctx.ToSvgParserResult())
			.Try();
	}
	
	private static Try<SvgParserContext> CreateSvgParserContext()
	{
		return () => new SvgParserContext ();
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
				return new Result<SvgParserContext>(new SvgParseException("Invalid SVG content"));
			
			return ctx;
		};
	}
	
	private static Try<SvgParserContext> ParseSvgData(SvgParserContext ctx)
	{
		return () => ctx.SvgRootElement == null
			? new Result<SvgParserContext>(new SvgParseException("SvgRootElement was null"))
			: SvgCrawler.ValidateAndExtractData(ctx.SvgRootElement, ctx);
	}
}
