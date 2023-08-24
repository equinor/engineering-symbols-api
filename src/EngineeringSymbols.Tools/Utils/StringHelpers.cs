using System.Text;

namespace EngineeringSymbols.Tools.Utils;

public static class StringHelpers
{
    public static string RemoveAllWhitespaceExceptSingleSpace(string input)
    {
        var result = new StringBuilder();

        var lastWasSpace = false;
        
        for (var i = 0; i < input.Length; i++)
        {
            var c = input[i];
            if (c == '\t' || c == ' ' && lastWasSpace)
            {
                continue;
            }

            lastWasSpace = c == ' ';

            result.Append(c);
        }

        return result.ToString().Trim();
    }
}