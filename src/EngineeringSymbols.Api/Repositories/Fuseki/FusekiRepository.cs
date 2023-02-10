using System.Globalization;
using System.Text;
using EngineeringSymbols.Api.Endpoints;
using EngineeringSymbols.Api.Entities;
using EngineeringSymbols.Api.Utils;
using LanguageExt.Pretty;
using VDS.RDF;
using VDS.RDF.Parsing;

namespace EngineeringSymbols.Api.Repositories.Fuseki;

public static class RdfConst
{
    public const string BaseIri = "http://rdf.engineering-symbols.no";
    
    public const string Symbol = "symbol";
    public const string Connector = "connector";
    public const string Prop = "prop";

    public const string SymbolTypeIri = BaseIri + "/types/symbol";
    
    public const string SymbolIri = BaseIri + "/lib/symbols/";
    public const string ConnectorIri = BaseIri + "/lib/connectors/";
    public const string PropIri = BaseIri + "/props/";
    
    public const string SymbolPrefix = $"PREFIX {Symbol}: <{SymbolIri}>";
    public const string ConnectorPrefix = $"PREFIX {Connector}: <{ConnectorIri}>";
    public const string PropPrefix = $"PREFIX {Prop}: <{PropIri}>";

    public const string RdfIri = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";
    public const string RdfsIri = "http://www.w3.org/2000/01/rdf-schema#";
    
    public const string AllPrefixes = $@"PREFIX rdf: <{RdfIri}>
PREFIX rdfs: <{RdfsIri}>
{SymbolPrefix}
{ConnectorPrefix}
{PropPrefix}";
    
}

public static class ESProp
{
    public const string IsType = "type";
    public const string IsTypeIri = $"{RdfConst.RdfIri}{IsType}";
    public const string IsTypePrefix = $"{RdfConst.RdfIri}:{IsType}";
    
    public const string HasLabel = "label";
    public const string HasLabelIri = $"{RdfConst.RdfsIri}{HasLabel}";
    public const string HasLabelIriPrefix = $"{RdfConst.RdfsIri}:{HasLabel}";
    
    public const string HasName = "hasName";
    public const string HasNameIri = $"{RdfConst.PropIri}{HasName}";
    public const string HasNameIriPrefix = $"{RdfConst.Prop}:{HasName}";
    
    public const string HasSourceFilename = "hasSourceFilename";
    public const string HasSourceFilenameIri = $"{RdfConst.PropIri}{HasSourceFilename}";
    public const string HasSourceFilenameIriPrefix = $"{RdfConst.Prop}:{HasSourceFilename}";
    
    public const string HasGeometry = "hasGeometry";
    public const string HasGeometryIri = $"{RdfConst.PropIri}{HasGeometry}";
    public const string HasGeometryIriPrefix = $"{RdfConst.Prop}:{HasGeometry}";
    
    public const string HasSvg = "hasSvg";
    public const string HasSvgIri = $"{RdfConst.PropIri}{HasSvg}";
    public const string HasSvgIriPrefix = $"{RdfConst.Prop}:{HasSvg}";
    
    public const string HasSvgBase64 = "hasSvgBase64";
    public const string HasSvgBase64Iri = $"{RdfConst.PropIri}{HasSvgBase64}";
    public const string HasSvgBase64IriPrefix = $"{RdfConst.Prop}:{HasSvgBase64}";
    
    public const string HasWidth = "width";
    public const string HasWidthIri = $"{RdfConst.PropIri}{HasWidth}";
    public const string HasWidthIriPrefix = $"{RdfConst.Prop}:{HasWidth}";
    
    public const string HasHeight = "height";
    public const string HasHeightIri = $"{RdfConst.PropIri}{HasHeight}";
    public const string HasHeightIriPrefix = $"{RdfConst.Prop}:{HasHeight}";
    
    public const string HasOwner = "owner";
    public const string HasOwnerIri = $"{RdfConst.PropIri}{HasOwner}";
    public const string HasOwnerIriPrefix = $"{RdfConst.Prop}:{HasOwner}";
    
    public const string HasConnector = "hasConnector";
    public const string HasConnectorIri = $"{RdfConst.PropIri}{HasConnector}";
    public const string HasConnectorIriPrefix = $"{RdfConst.Prop}:{HasConnector}";
    
    public const string HasDateCreated = "dateCreated";
    public const string HasDateCreatedIri = $"{RdfConst.PropIri}{HasDateCreated}";
    public const string HasDateCreatedIriPrefix = $"{RdfConst.Prop}:{HasDateCreated}";
    
    public const string HasDateUpdated = "dateUpdated";
    public const string HasDateUpdatedIri = $"{RdfConst.PropIri}{HasDateUpdated}";
    public const string HasDateUpdatedIriPrefix = $"{RdfConst.Prop}:{HasDateUpdated}";
    
    public const string HasPositionX = "positionX";
    public const string HasPositionXIri = $"{RdfConst.PropIri}{HasPositionX}";
    public const string HasPositionXIriPrefix = $"{RdfConst.Prop}:{HasPositionX}";
    
    public const string HasPositionY = "positionY";
    public const string HasPositionYIri = $"{RdfConst.PropIri}{HasPositionY}";
    public const string HasPositionYIriPrefix = $"{RdfConst.Prop}:{HasPositionY}";
    
    public const string HasDirection = "direction";
    public const string HasDirectionYIri = $"{RdfConst.PropIri}{HasDirection}";
    public const string HasDirectionYIriPrefix = $"{RdfConst.Prop}:{HasDirection}";
}

public static class ESPred
{
    public const string HasLabel = $"rdfs:label";
    public const string HasName = $"{RdfConst.Prop}:hasName";
    public const string HasGeometry = $"{RdfConst.Prop}:hasGeometry";
    public const string HasSvg = $"{RdfConst.Prop}:hasSvg";
    public const string HasWidth = $"{RdfConst.Prop}:width";
    public const string HasHeight = $"{RdfConst.Prop}:height";
    public const string HasOwner = $"{RdfConst.Prop}:owner";
    public const string HasConnector = $"{RdfConst.Prop}:hasConnector";
    public const string HasDateCreated = $"{RdfConst.Prop}:dateCreated";
    public const string HasDateUpdated = $"{RdfConst.Prop}:dateUpdated";
    public const string HasPositionX = $"{RdfConst.Prop}:positionX";
    public const string HasPositionY = $"{RdfConst.Prop}:positionY";
    public const string HasDirection = $"{RdfConst.Prop}:direction";
    

    
}

public class FusekiRepository : IEngineeringSymbolRepository
{
    private readonly IFusekiService _fuseki;
    
    public FusekiRepository(IFusekiService fuseki)
    {
        _fuseki = fuseki;
    }

    public TryAsync<IEnumerable<string>> GetAllEngineeringSymbolsAsync() =>
        async () =>
        {
            const string query = $@"PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>
SELECT ?symbolIri WHERE {{ ?symbolIri rdf:type <{RdfConst.SymbolTypeIri}> }}";

            var httpResponse = await _fuseki.QueryAsync(query, "text/csv");

            var stringContent = await httpResponse.Content.ReadAsStringAsync();

            var iris = stringContent
                .Split("\n")
                .Select(s => s.Replace("\r", "").Trim())
                .Filter(s => !string.IsNullOrWhiteSpace(s))
                .Skip(1)
                .ToArray();

            return iris;
        };

    public TryAsync<EngineeringSymbol> GetEngineeringSymbolByIdAsync(string id) => 
        async () =>
        {
            var query = $@"{RdfConst.SymbolPrefix}
{RdfConst.ConnectorPrefix}
{RdfConst.PropPrefix}

CONSTRUCT
{{
    {RdfConst.Symbol}:{id} ?p ?o .
    ?connector ?cp ?co .
}}
WHERE
{{
    {RdfConst.Symbol}:{id} ?p ?o .
    {RdfConst.Symbol}:{id} {ESPred.HasConnector} ?connector .
    ?connector ?cp ?co .
}}";
            Console.WriteLine("GET SYMBOL QUERY:");
            Console.WriteLine(query);
            
            var httpResponse = await _fuseki.QueryAsync(query, "text/turtle");
            var stringContent = await httpResponse.Content.ReadAsStringAsync();
            
            var symbolV = RdfParser.TurtleToEngineeringSymbol(stringContent);

            return symbolV.Match(symbol => symbol, seq => throw new Exception("Parse err"));
        };

    public TryAsync<EngineeringSymbol> InsertEngineeringSymbolAsync(EngineeringSymbolCreateDto createDto)
    {
        return async () =>
        {
            var nfi = new NumberFormatInfo {NumberDecimalSeparator = "."};
            var symbolId = RepoUtils.GetRandomString();
            var sub = $"{RdfConst.Symbol}:{symbolId}";
            
            var connectorTurtle = createDto.Connectors.Map(connector =>
            {
                var cIri = $"{RdfConst.Connector}:{symbolId}_C_{connector.Id}";
                
                return $@"  {sub} {ESPred.HasConnector} {cIri} .
    {cIri} {ESPred.HasName} ""{connector.Id}"" .
    {cIri} {ESPred.HasPositionX} ""{connector.RelativePosition.X.ToString(nfi)}"" .
    {cIri} {ESPred.HasPositionY} ""{connector.RelativePosition.Y.ToString(nfi)}"" .
    {cIri} {ESPred.HasDirection} ""{connector.Direction}"" .";
            }).ToList();

            //var svgStr = $@"{createDto.SvgString.Replace("\"","\"")}";
            var svgStrBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(createDto.SvgString));
            
            var query = RdfConst.AllPrefixes + Environment.NewLine +
                        $@"
INSERT DATA {{
    {sub} rdf:type <{RdfConst.SymbolTypeIri}> .
    {sub} {ESPred.HasDateCreated} ""{DateTimeOffset.UtcNow.ToString("O")}"" .
    {sub} {ESPred.HasDateUpdated} ""{DateTimeOffset.MinValue.ToString("O")}"" .
    {sub} {ESPred.HasLabel} ""{symbolId}"" .
    {sub} {ESPred.HasGeometry} ""{createDto.GeometryString}"" .
    {sub} {ESProp.HasSvgBase64IriPrefix} ""{svgStrBase64}"" .
    {sub} {ESPred.HasWidth} ""{createDto.Width.ToString(nfi)}"" .
    {sub} {ESPred.HasHeight} ""{createDto.Height.ToString(nfi)}"" .
    {sub} {ESPred.HasOwner} ""{createDto.Owner}"" .
    {sub} {ESProp.HasSourceFilenameIriPrefix} ""{createDto.Filename}"" .
{string.Join(Environment.NewLine, connectorTurtle)}
}}";
            Console.WriteLine("   ---  QUERY  ---");
            Console.WriteLine(query);

            try
            {
                var res = await _fuseki.UpdateAsync(query);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw new RepositoryException(e.Message);
            }
            
            
            //var sym = await GetEngineeringSymbolByIdAsync(symbolId).Try();

            //return sym;
            return new Result<EngineeringSymbol>(new EngineeringSymbol());
        };
    }
}