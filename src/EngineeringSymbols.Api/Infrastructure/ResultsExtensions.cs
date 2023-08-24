using EngineeringSymbols.Api.Repositories.Fuseki;

namespace EngineeringSymbols.Api.Infrastructure;
using System.Net.Mime;
using System.Text;

static class ResultsExtensions
{
    public static IResult Fuseki(this IResultExtensions resultExtensions, FusekiRawResponse response)
    {
        ArgumentNullException.ThrowIfNull(resultExtensions);

        return new FusekiResult(response);
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