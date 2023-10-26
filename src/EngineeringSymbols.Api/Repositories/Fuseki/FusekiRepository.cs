using EngineeringSymbols.Tools.Constants;
using EngineeringSymbols.Tools.Entities;
using EngineeringSymbols.Tools.Rdf;

namespace EngineeringSymbols.Api.Repositories.Fuseki;

public class FusekiRepository : IEngineeringSymbolRepository
{
	private readonly ILogger _logger;
	private readonly IFusekiService _fuseki;

	public FusekiRepository(IFusekiService fuseki, ILoggerFactory loggerFactory)
	{
		_fuseki = fuseki;
		_logger = loggerFactory.CreateLogger("FusekiRepository");
	}

	private async Task<Result<T>> FusekiRequestErrorResult<T>(HttpResponseMessage httpResponse, string? message = null, string? sparqlQuery = null)
	{
		var reqUri = httpResponse.RequestMessage?.RequestUri?.AbsoluteUri ?? "?";
		var method = httpResponse.RequestMessage?.Method.Method ?? "";

		var stringContent = message ?? await httpResponse.Content.ReadAsStringAsync();

		_logger.LogError(
			"Repository request to '{RequestUri}' failed: Status {StatusCode} {ReasonPhrase} (Uri: {Method} {AbsoluteUri})\nSparql Query: \n{SparqlQuery}. Message: {Msg}",
			 reqUri, httpResponse.StatusCode, httpResponse.ReasonPhrase, method, reqUri, sparqlQuery, stringContent);

		return new Result<T>(new RepositoryException($"Repository request to '{reqUri}' failed: Status {httpResponse.ReasonPhrase}. Message: {stringContent}"));
	}

	public TryAsync<string> GetAllEngineeringSymbolsAsync(bool onlyLatestVersion = true, bool onlyPublished = true) =>
		async () =>
		{
			var query = SparqlQueries.GetAllSymbolsConstructQuery(onlyLatestVersion, onlyPublished);

			var httpResponse =
				await _fuseki.QueryAsync(query, "application/ld+json"); //"application/sparql-results+json" "text/csv"

			var stringContent = await httpResponse.Content.ReadAsStringAsync();

			if (!httpResponse.IsSuccessStatusCode)
			{
				return await FusekiRequestErrorResult<string>(httpResponse, stringContent, sparqlQuery: query);
			}

			return stringContent;
		};
    
	public TryAsync<string> GetEngineeringSymbolByIdAsync(string id, bool onlyPublished = true)
		=> TryAsync(() => Task.FromResult(SparqlQueries.GetEngineeringSymbolByIdQuery(id, onlyPublished)))
			.Bind(_getEngineeringSymbolByQueryAsync);

	public TryAsync<string> GetEngineeringSymbolByIdentifierAsync(string identifier, bool onlyPublished = true)
		=> TryAsync(() => Task.FromResult(SparqlQueries.GetEngineeringSymbolByIdentifierQuery(identifier, onlyPublished)))
			.Bind(_getEngineeringSymbolByQueryAsync);

	private TryAsync<string> _getEngineeringSymbolByQueryAsync(string query) =>
		async () =>
		{
			var httpResponse = await _fuseki.QueryAsync(query, "application/ld+json"); //"text/turtle"

			var stringContent = await httpResponse.Content.ReadAsStringAsync();

			if (!httpResponse.IsSuccessStatusCode)
			{
				return await FusekiRequestErrorResult<string>(httpResponse, stringContent, sparqlQuery: query);
			}

			return stringContent;
		};

	public TryAsync<EngineeringSymbol> PutEngineeringSymbolAsync(EngineeringSymbol symbol) =>
		async () =>
		{
			var graph = $"{Ontology.SymbolIri}{symbol.Id}";

			var turtleString = await SymbolGraphHelper.EngineeringSymbolToTurtleString(symbol);

			_logger.LogInformation("Put Graph:\n{SymbolGraphTurtle}", turtleString);

			var httpResponse = await _fuseki.PutGraphAsync(graph, turtleString);

			if (!httpResponse.IsSuccessStatusCode)
			{
				return await FusekiRequestErrorResult<EngineeringSymbol>(httpResponse, sparqlQuery: turtleString);
			}

			return symbol with { };
		};

	public TryAsync<Unit> UpdateSymbolStatusAsync(SymbolStatusInfo statusInfo)
		=> async () =>
		{
			var query = SparqlQueries.UpdateEngineeringSymbolStatusQuery(statusInfo);

			_logger.LogInformation("Updating Status with query:\n{Query}", query);

			var httpResponse = await _fuseki.UpdateAsync(query);

			if (!httpResponse.IsSuccessStatusCode)
			{
				return await FusekiRequestErrorResult<Unit>(httpResponse, sparqlQuery: query);
			}

			return Unit.Default;
		};

	public TryAsync<Unit> DeleteEngineeringSymbolAsync(string id) =>
		SymbolExistsByIdAsync(id)
			.Bind(exists => new TryAsync<Unit>(async () =>
			{
				if (!exists)
				{
					return new Result<Unit>(new RepositoryException(RepositoryOperationError.EntityNotFound));
				}

				var query = SparqlQueries.DeleteEngineeringSymbolByIdQuery(id);

				var httpResponse = await _fuseki.UpdateAsync(query);

				return httpResponse.IsSuccessStatusCode
					? Unit.Default
					: await FusekiRequestErrorResult<Unit>(httpResponse, sparqlQuery: query);
			}));


	public TryAsync<FusekiRawResponse> FusekiQueryAsync(string query, string accept)
		=> new TryAsync<HttpResponseMessage>(
				async () => await _fuseki.QueryAsync(query, accept))
			.Bind(_ToFusekiRawResponse);

	public TryAsync<FusekiRawResponse> FusekiUpdateAsync(string query, string accept)
		=> new TryAsync<HttpResponseMessage>(
				async () => await _fuseki.UpdateAsync(query, accept))
			.Bind(_ToFusekiRawResponse);


	private static TryAsync<FusekiRawResponse> _ToFusekiRawResponse(HttpResponseMessage httpResponse)
		=> async () =>
		{
			var content = await httpResponse.Content.ReadAsStringAsync();

			return new FusekiRawResponse
			{
				StatusCode = (int)httpResponse.StatusCode,
				Content = content,
				ContentType = httpResponse.Content.Headers?.ContentType?.MediaType ?? ContentTypes.Plain
			};
		};

	private TryAsync<bool> SymbolExistsByIdAsync(string? id) => async () =>
	{
		if (id == null)
		{
			return new Result<bool>(new RepositoryException("Symbol Id was null"));
		}

		var query = SparqlQueries.SymbolExistByIdQuery(id);

		var httpResponse = await _fuseki.QueryAsync(query);

		if (!httpResponse.IsSuccessStatusCode)
		{
			return await FusekiRequestErrorResult<bool>(httpResponse, sparqlQuery: query);
		}

		var res = await httpResponse.Content.ReadFromJsonAsync<FusekiAskResponse>();

		if (res == null)
		{
			return new Result<bool>(new RepositoryException("Failed to prove the symbol's existence"));
		}

		return res.Boolean;
	};
}