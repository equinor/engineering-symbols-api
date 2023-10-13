using System.Reflection;
using EngineeringSymbols.Tools.Utils;
using Newtonsoft.Json.Linq;

namespace EngineeringSymbols.Api.Services;

public static class SymbolFrames
{
    public static JToken FramePublic { get; private set; } = new JObject();
    public static JToken FrameInternal { get; private set; } = new JObject();

    private static string GetFilePath(string absOrRelativePath)
    {
        if (Path.IsPathRooted(absOrRelativePath)) return absOrRelativePath;
        
        // Get the directory of the executable
        var exePath = Assembly.GetExecutingAssembly().Location;
        var exeDirectory = Path.GetDirectoryName(exePath);

        // Build the path to the file you want to read
        return Path.Combine(exeDirectory, absOrRelativePath);
    }

    public static async Task Load()
    {
        var baseFrameJson = await File.ReadAllTextAsync(GetFilePath("frames/SymbolFrameBase.json"));
        var publicFrame = JObject.Parse(baseFrameJson);
        
        var publicFrameJson = await File.ReadAllTextAsync(GetFilePath("frames/PublicFieldsFrame.json"));
        var pubDict = JObject.Parse(publicFrameJson);
        
        foreach (var property in pubDict.Properties())
        {
            if (publicFrame.ContainsKey(property.Name))
            {
                throw new Exception("");
            }
            publicFrame.Add(property.Name, property.Value);
        }

        FramePublic = publicFrame;
        
        var internalFrame = (JObject)publicFrame.DeepClone();
        var internalFrameJson = await File.ReadAllTextAsync(GetFilePath("frames/InternalFieldsFrame.json"));
        var internalDict = JObject.Parse(internalFrameJson);

        foreach (var property in internalDict.Properties())
        {
            if (internalFrame.ContainsKey(property.Name))
            {
                throw new Exception("");
            }
            
            internalFrame.Add(property.Name, property.Value);
        }

        FrameInternal = internalFrame;
    }
}