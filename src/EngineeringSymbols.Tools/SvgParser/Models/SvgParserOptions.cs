namespace EngineeringSymbols.Tools.SvgParser.Models;

public class SvgParserOptions
{
	public bool RemoveAnnotations { get; set; } = true;
	public string AnnotationsElementId { get; set; } = "annotations";
	public bool IncludeRawSvgString { get; set; }
	public string FillColor { get; set; } = "#231f20";
	public string ConnectorFillColor { get; set; } = "#FF0000";
}