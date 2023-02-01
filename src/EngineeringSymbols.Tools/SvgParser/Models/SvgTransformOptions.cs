namespace EngineeringSymbols.Tools.SvgParser;

public class SvgTransformOptions
{
	public string? SymbolId { get; set; }
	public string SymbolName { get; set; } = string.Empty;

	public bool RemoveAnnotations { get; set; } = true;

	public string AnnotationsElementId { get; set; } = "annotations";

	public bool IncludeSvgString = true;

	public bool IncludeRawSvgString = false;

	public string StrokeColor { get; set; } = "#231f20";

	public string FillColor { get; set; } = "#231f20";
	public string ConnectorFillColor { get; set; } = "#FF0000";
}