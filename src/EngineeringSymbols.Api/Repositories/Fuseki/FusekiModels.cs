namespace EngineeringSymbols.Api.Repositories.Fuseki;

public class FusekiResult
{
    public List<Dictionary<string, FusekiTriplet>> Bindings { get; set; } = new();
}

public class FusekiTriplet
{
    public string? Type { get; set; }
    public string? Datatype { get; set; }
    public string? Value { get; set; }
}

public class FusekiHead
{
    public List<string> Vars { get; set; } = new();
}

public class FusekiSelectResponse
{
    public FusekiHead Head { get; set; } = new();
    public FusekiResult Results { get; set; } = new();
}

public class FusekiAskResponse
{
    public FusekiHead Head { get; set; } = new();
    public bool Boolean { get; set; }
}
