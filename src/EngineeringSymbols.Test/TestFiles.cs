namespace EngineeringSymbols.Test;



public static class TestFilePaths
{
    private const string Root = "./SvgTestFiles/";
    
    private const string ValidPath = Root + "Valid/";
    private const string InvalidPath = Root + "Invalid/";
    
    // VALID FILES
    public const string File1 = ValidPath + "PV003B.svg";
    public const string File2 = ValidPath + "PT002A_Option1.svg";
    
    // INVALID FILES
    public const string InvalidRootElement = InvalidPath + "INVALID_Root_Element.svg";
    public const string InvalidConnectors = InvalidPath + "PV003B_Invalid_Connector.svg";
    public const string InvalidViewBox = InvalidPath + "PV003B_Invalid_viewbox.svg";

    // INVALID file paths
    public const string AFileThatNotExists = "./the_best.svg";
}