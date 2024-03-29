using EngineeringSymbols.Api.Repositories.Fuseki;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;

namespace EngineeringSymbols.Api.Infrastructure;

internal static class ResultsExtensions
{
    public static IResult Fuseki(this IResultExtensions resultExtensions, FusekiRawResponse response)
    {
        ArgumentNullException.ThrowIfNull(resultExtensions);
        return new FusekiResult(response);
    }
    
     public static IResult EngineeringSymbol(this IResultExtensions resultExtensions, JObject jObject)
     {
         ArgumentNullException.ThrowIfNull(resultExtensions);

         var settings = new JsonSerializerSettings
         {
             Formatting = Formatting.None,
             NullValueHandling = NullValueHandling.Ignore,
         };
         
         var content = JsonConvert.SerializeObject(jObject, settings);

         return Results.Text(content, contentType: ContentTypes.JsonLd);
     }
}

class FusekiResult : IResult
{
    private readonly FusekiRawResponse _response;

    public FusekiResult(FusekiRawResponse response)
    {
        _response = response;
    }

    public Task ExecuteAsync(HttpContext httpContext)
    {
        httpContext.Response.StatusCode = _response.StatusCode;
        httpContext.Response.ContentType = _response.ContentType;
        httpContext.Response.ContentLength = Encoding.UTF8.GetByteCount(_response.Content);
        return httpContext.Response.WriteAsync(_response.Content);
    }
}