using System.Net.Http.Headers;
using System.Text;

namespace EngineeringSymbols.Api.Services;

public class FusekiService : IFusekiService
{
    private readonly HttpClient _httpClient;

    public FusekiService(HttpClient httpClient, IConfiguration config)
    {
        var fusekiServer = config.GetSection("FusekiServer").Value;
        if (fusekiServer == null)
            throw new Exception("FusekiServer is missing from appsettings.json");
        
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(fusekiServer);
        _httpClient.DefaultRequestHeaders.Accept.Clear();
        _httpClient.DefaultRequestHeaders.Accept.ParseAdd("application/sparql-results+json");
    }

    public async Task<HttpResponseMessage> QueryAsync(string sparqlQuery, string? accept)
    {
        var acc = accept ?? "application/sparql-results+json; charset=utf-8";
        _httpClient.DefaultRequestHeaders.Accept.Clear();
        _httpClient.DefaultRequestHeaders.Accept.ParseAdd(acc);
        
        var response =  await _httpClient.PostAsync("sparql", 
            new FormUrlEncodedContent(new KeyValuePair<string, string>[]
            {
                new("query", sparqlQuery)
            }));

        return response;
    }
    
    public async Task<HttpResponseMessage> UpdateAsync(string updateQuery)
    {
        var response =  await _httpClient.PostAsync("update", 
            new FormUrlEncodedContent(new KeyValuePair<string, string>[]
            {
                new("update", updateQuery)
            }));

        return response;
    }
}