namespace EngineeringSymbols.Test;



public static class TestFilePaths
{
    private const string Root = "./SvgTestFiles/";
    // VALID FILES
    public const string File1 = Root + "PV003B.svg";
    public const string File2 = Root + "PT002A_Option1.svg";
    
    // INVALID FILES
    public const string InvalidRootElement = Root + "INVALID_Root_Element.svg";
    public const string InvalidConnectors = Root + "PV003B_Invalid_Connector.svg";
    public const string InvalidViewBox = Root + "PV003B_Invalid_viewbox.svg";

    // INVALID file paths
    public const string AFileThatNotExists = "./the_best.svg";
}