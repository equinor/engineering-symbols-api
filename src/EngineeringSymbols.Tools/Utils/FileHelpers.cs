using System.Reflection;

namespace EngineeringSymbols.Tools.Utils;

public static class FileHelpers
{
    public static async Task<string> GetJsonLdFrame()
    {
        // Get the directory of the executable
        var exePath = Assembly.GetExecutingAssembly().Location;
        var exeDirectory = Path.GetDirectoryName(exePath);

        // Build the path to the file you want to read
        var filePath = Path.Combine(exeDirectory, "Repositories/Fuseki/SymbolFrame.json");

        return await File.ReadAllTextAsync(filePath);
    }
}