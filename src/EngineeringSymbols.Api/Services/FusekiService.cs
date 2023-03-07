using System.Net.Http.Headers;
using System.Text;

namespace EngineeringSymbols.Api.Services;

public class FusekiService : IFusekiService
{
	private readonly HttpClient _httpClient;

	public FusekiService(HttpClient httpClient, IConfiguration config)
	{
		var fusekiServers = config.GetSection("FusekiServers").Get<List<FusekiServerSettings>>();
		
		if (fusekiServers == null || fusekiServers.Count == 0)
			throw new Exception("'FusekiServers' is missing or empty in appsettings.json");

		var serverBaseUrl = fusekiServers[0].BaseUrl;
		
		if (serverBaseUrl == null)
			throw new Exception("'FusekiServer BaseUrl' in appsettings.json is null");
		
		_httpClient = httpClient;
		_httpClient.BaseAddress = new Uri(serverBaseUrl);
		_httpClient.DefaultRequestHeaders.Accept.Clear();
	}

	public async Task<HttpResponseMessage> QueryAsync(string sparqlQuery, string? accept)
	{
		var request = new HttpRequestMessage(HttpMethod.Post, new Uri("ds/query", UriKind.Relative))
		{
			Content = new FormUrlEncodedContent(new KeyValuePair<string, string>[]
			{
				new("query", sparqlQuery)
			}),
			Headers = { Accept = { new MediaTypeWithQualityHeaderValue(accept ?? "application/sparql-results+json") }}
		};
		
		return await _httpClient.SendAsync(request);
	}

	public async Task<HttpResponseMessage> UpdateAsync(string updateQuery, string? accept)
	{
		var request = new HttpRequestMessage(HttpMethod.Post, new Uri("ds/update", UriKind.Relative))
		{
			Content = new FormUrlEncodedContent(new KeyValuePair<string, string>[]
			{
				new("update", updateQuery)
			}),
			Headers = { Accept = { new MediaTypeWithQualityHeaderValue(accept ?? "application/sparql-results+json") }}
		};

		return await _httpClient.SendAsync(request);
	}
}