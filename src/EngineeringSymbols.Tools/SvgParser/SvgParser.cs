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

	private static Result<SvgParserResult> ParseSvgFlow(Func<Try<SvgParserContext>> xElementLoader)
	{
		return xElementLoader()
			.Bind(ValidateRootElement)
			.Bind(ParseSvgData)
			.Map(ctx => ctx.ToSvgParserResult())
			.Try();
	}

	private static Func<Try<SvgParserContext>> LoadRootXElementFromString(string svgString)
	{
		return () => new Try<XElement>(() =>
		{
			try
			{
				return XElement.Parse(svgString);
			}
			catch (Exception)
			{
				return new Result<XElement>(new SvgParseException($"Failed to parse SVG as XML element."));
			}

		})
		.Map(el => new SvgParserContext(el));
	}

	private static Func<Try<SvgParserContext>> LoadRootXElementFromFile(string filePath)
	{
		return () => new Try<string>(() =>
			{

				var fullPath = "";

				try
				{
					fullPath = Path.GetFullPath(filePath);
				}
				catch (Exception)
				{
					return new Result<string>(new SvgParseException("Invalid file path"));
				}

				if (!File.Exists(fullPath))
				{
					return new Result<string>(new SvgParseException($"Could not find SVG at path '{filePath}'."));
				}

				return fullPath;
			})
			.Bind(fullPath => new Try<XElement>(() =>
			{
				try
				{
					return XElement.Load(fullPath);
				}
				catch (Exception)
				{
					return new Result<XElement>(new SvgParseException($"Failed to load SVG from path: {filePath}"));
				}
			}))
			.Map(el => new SvgParserContext(el));
	}


	private static Try<SvgParserContext> ValidateRootElement(SvgParserContext ctx)
	{
		return () => ctx.SvgRootElement.Name.LocalName != "svg"
			? new Result<SvgParserContext>(new SvgParseException("Invalid SVG content"))
			: ctx;
	}

	private static Try<SvgParserContext> ParseSvgData(SvgParserContext ctx)
	{
		return () => SvgCrawler.ValidateAndExtractData(ctx.SvgRootElement, ctx);
	}
}
