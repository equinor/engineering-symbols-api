namespace EngineeringSymbols.Api.Models;

public class FusekiServerSettings
{
    public string? Name { get; set; }
    
    public string? DatasetUrl { get; set; }
    
    public List<string>? Scopes { get; set; }
}