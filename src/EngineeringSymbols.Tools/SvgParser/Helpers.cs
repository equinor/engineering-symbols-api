using System.Text.RegularExpressions;

namespace EngineeringSymbols.Tools.SvgParser;

internal static class Helpers
{
	public static string GetSymbolId(string filename)
	{
		var fname = filename.Split(".")[0];

		if (fname == null)
			throw new Exception($"Failed to parse SymbolId from filename");

		var rgx = new Regex("[^a-zA-Z0-9 -]");

		return rgx.Replace(fname, "").Replace("-", "_");
	}
}