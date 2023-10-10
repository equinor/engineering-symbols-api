using EngineeringSymbols.Api.Repositories.Fuseki;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;

namespace EngineeringSymbols.Api.Infrastructure;

static class ResultsExtensions
{
    public static IResult Fuseki(this IResultExtensions resultExtensions, FusekiRawResponse response)
    {
        ArgumentNullException.ThrowIfNull(resultExtensions);
        return new FusekiResult(response);
    }
    
     public static IResult EngineeringSymbol(this IResultExtensions resultExtensions, JObject jObject)
     {
         ArgumentNullException.ThrowIfNull(resultExtensions);
         return new EngineeringSymbolResult(jObject);
     }
}


class EngineeringSymbolResult : IResult
{
    private readonly JObject _jObjectResponse;
    public EngineeringSymbolResult(JObject response)
    {
        _jObjectResponse = response;
    }
    
    public Task ExecuteAsync(HttpContext httpContext)
    {
        var content = _jObjectResponse.ToString(Formatting.None);
        httpContext.Response.StatusCode = StatusCodes.Status200OK;
        httpContext.Response.ContentType = ContentTypes.JsonLd;
        httpContext.Response.ContentLength = Encoding.UTF8.GetByteCount(content);
        return httpContext.Response.WriteAsync(content);
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