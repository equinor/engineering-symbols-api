using System.Globalization;
using EngineeringSymbols.Api.Repositories;
using EngineeringSymbols.Tools;
using EngineeringSymbols.Tools.Constants;
using EngineeringSymbols.Tools.Entities;
using EngineeringSymbols.Tools.Models;
using EngineeringSymbols.Tools.Validation;
using Microsoft.Identity.Abstractions;
using Newtonsoft.Json.Linq;
using VDS.RDF.JsonLd;

namespace EngineeringSymbols.Api.Services;

public class EngineeringSymbolService : IEngineeringSymbolService
{
	private readonly IEngineeringSymbolRepository _repo;

	private readonly IDownstreamApi _downstreamApi;

	private readonly ILogger _logger;

	public EngineeringSymbolService(IConfiguration config,
			IDownstreamApi downstreamApi, IEngineeringSymbolRepository repo,
		ILoggerFactory loggerFactory)
	{
		_repo = repo;
		_downstreamApi = downstreamApi;
		_logger = loggerFactory.CreateLogger("EngineeringSymbolService");
	}

	private static Func<string, TryAsync<JObject>> AddFraming(bool publicVersion)
	{
		return jsonLdContent => () =>
		{
			var input = JToken.Parse(jsonLdContent, new JsonLoadSettings() { });
			var frame = publicVersion ? SymbolFrames.FramePublic : SymbolFrames.FrameInternal;
			var framed = JsonLdProcessor.Frame(input, frame, new JsonLdProcessorOptions { Explicit = true });
			return Task.FromResult(new Result<JObject>(framed));
		};
	}

	public TryAsync<JObject> GetSymbolsAsync(bool onlyLatestVersion, bool publicVersion)
	{
		return _repo.GetAllEngineeringSymbolsAsync(onlyLatestVersion, publicVersion)
			.Bind(AddFraming(publicVersion));
	}

	public TryAsync<JObject> GetSymbolByIdOrIdentifierAsync(string idOrIdentifier, bool publicVersion)
	{
		return new Try<(string, bool)>(() =>
			{
				if (Guid.TryParse(idOrIdentifier, out _)) return (idOrIdentifier, true);

				var identifierValidator = new EngineeringSymbolIdentifierValidator();

				return identifierValidator.Validate(idOrIdentifier).IsValid
					? (idOrIdentifier, false)
					: new Result<(string, bool)>(new ValidationException(
						new Dictionary<string, string[]>
						{
							{"idOrIdentifier", new[] {"Value is not a valid GUID or Engineering Symbol Identifier."}}
						}));
			})
			.ToAsync()
			.Bind(idOrKeyTuple => idOrKeyTuple.Item2
				? _repo.GetEngineeringSymbolByIdAsync(idOrKeyTuple.Item1, publicVersion)
				: _repo.GetEngineeringSymbolByIdentifierAsync(idOrKeyTuple.Item1, publicVersion))
			.Bind(AddFraming(publicVersion));
	}

	public TryAsync<EngineeringSymbol> CreateSymbolAsync(EngineeringSymbolPutDto putDto, bool validationOnly)
	{
		return new TryAsync<EngineeringSymbol>(async () => putDto.ToInsertEntity())
			.Bind(symbol => !validationOnly
				? _repo.PutEngineeringSymbolAsync(symbol)
				: async () => symbol with { Id = "", DateTimeCreated = DateTime.UnixEpoch });
	}

	private static SymbolSlim ToSymbolSlim(JObject obj)
	{
		// "dc:identifier": "PP007A_alt1",
		// "pav:version": "1",
		// "pav:previousVersion": "...",
		// "esmde:status": "Draft",

		var id = (string)obj["@id"];
		var identifier = (string)obj[EsProp.IdentifierQName];
		var version = (string)obj[EsProp.VersionQName];

		string? isRevOf = null;

		if (obj[EsProp.PreviousVersionQName] is JObject jObj && jObj.ContainsKey("@id"))
		{
			isRevOf = (string)jObj["@id"];
		}

		var dateIssued = (string)obj[EsProp.DateIssuedQName];

		if (!DateTime.TryParse(dateIssued, CultureInfo.InvariantCulture, out var dateTimeParsed))
		{
			throw new RepositoryException("Failed to get existing DateTimeCreated from db.");
		}

		return new SymbolSlim(id, identifier, dateTimeParsed, version, isRevOf);
	}

	public TryAsync<EngineeringSymbol> UpdateSymbolAsync(string id, EngineeringSymbolPutDto putDto)
	{
		return _repo.GetEngineeringSymbolByIdAsync(id, false)
			.Bind(AddFraming(publicVersion: false))
			.Bind(existing => new TryAsync<EngineeringSymbol>(async () =>
				{
					if (!existing.ContainsKey("@id"))
					{
						return new Result<EngineeringSymbol>(
							new RepositoryException("Expected root level '@id' field."));
					}

					if (!Enum.TryParse<EngineeringSymbolStatus>((string?)existing.GetValue(EsProp.EditorStatusQName),
							out var currentStatus))
					{
						return new Result<EngineeringSymbol>(
							new RepositoryException(
								$"Failed to read/parse '{EsProp.EditorStatusQName}' from existing state."));
					}

					if (currentStatus == EngineeringSymbolStatus.Issued)
					{
						return new Result<EngineeringSymbol>(
							new ValidationException(
								$"Cannot modify symbol because it has status '{EngineeringSymbolStatus.Issued}'."));
					}

					var createdDateToken = existing.GetValue(EsProp.DateCreatedQName);

					if (createdDateToken == null)
					{
						return new Result<EngineeringSymbol>(
							new RepositoryException(
								$"Failed to read/parse '{EsProp.DateCreatedQName}' from existing state."));
					}

					var createdDate = (DateTime)createdDateToken;

					// These fields must be set by backend, other fields come from the putDto:  
					//  - Id  
					//  - Status => Dont update, just pass over (status is set by own endpoint)
					//  - Dates...

					// "esmde:id": "4d193516-dbaf-4829-afd9-e5f327bc2dc6",  
					// "esmde:oid": "d5327a96-0771-4c0c-9334-bd14a0d3cb09",  
					// "esmde:status": "Draft",  

					return putDto.ToInsertEntity() with
					{
						Id = id,
						Status = currentStatus,
						DateTimeCreated = createdDate,
						DateTimeModified = DateTime.UtcNow,
					};
				}
			)).Bind(_repo.PutEngineeringSymbolAsync);
	}


	public TryAsync<Unit> UpdateSymbolStatusAsync(string id, EngineeringSymbolStatusDto statusDto)
		=> _repo.GetEngineeringSymbolByIdAsync(id, false)
			.Bind(AddFraming(false))
			.Bind(sObj => new TryAsync<SymbolStatusInfo>(
				async () =>
				{
					if (!Enum.TryParse<EngineeringSymbolStatus>(statusDto.Status, out var newStatus) ||
						newStatus == EngineeringSymbolStatus.None)
					{
						var statusValues = Enum.GetValues(typeof(EngineeringSymbolStatus))
							.OfType<EngineeringSymbolStatus>()
							.Select(e => e.ToString()).ToList();

						var validValues = string.Join(", ",
							statusValues.Where(s => s != "None").Select(s => $"'{s}'"));

						return new Result<SymbolStatusInfo>(
							new ValidationException(new Dictionary<string, string[]>
							{
								{nameof(statusDto.Status), new[] {$"Invalid value. Valid values are: {validValues}."}},
							}));
					}

					var statusDb = (string?)sObj.GetValue(EsProp.EditorStatusQName);

					if (!Enum.TryParse<EngineeringSymbolStatus>(statusDb, out var currentStatus))
					{
						return new Result<SymbolStatusInfo>(
							new RepositoryException("Failed to get existing status from db."));
					}

					if (currentStatus == EngineeringSymbolStatus.Issued)
					{
						return new Result<SymbolStatusInfo>(
							new ValidationException("The symbol has already been issued."));
					}

					var identifier = (string?)sObj.GetValue(EsProp.IdentifierQName);

					if (identifier == null)
					{
						return new Result<SymbolStatusInfo>(
							new RepositoryException("Failed to get existing Identifier from db."));
					}

					return new SymbolStatusInfo(id, identifier, newStatus, null, null);

				}))
			.Bind(ResolveIssuedVersionAsync)
			.Bind(_repo.UpdateSymbolStatusAsync)
			.Bind(_ => PublishToCommonLibAsync(id, statusDto));

	public TryAsync<SymbolStatusInfo> ResolveIssuedVersionAsync(SymbolStatusInfo statusInfo)
	{
		return async () =>
		{
			if (statusInfo.Status != EngineeringSymbolStatus.Issued)
				return statusInfo;

			// Check if Identifier exists
			var existsResult = await _repo.GetEngineeringSymbolByIdentifierAsync(statusInfo.Identifier, true)
				.Bind(AddFraming(false))
				.Try();

			Exception? existsException = null;

			// Check if multiple, single or none...
			var ancestors = existsResult.Match<List<SymbolSlim>>(jObject =>
			{
				var res = new List<SymbolSlim>();

				// Top level symbol graph
				if (jObject.ContainsKey("@id"))
					res.Add(ToSymbolSlim(jObject));
				else if (jObject.TryGetValue("@graph", out var value))
					res.AddRange(from g in value as JArray select ToSymbolSlim(g as JObject));

				return res;
			}, exception =>
			{
				if (exception is not RepositoryException
					{
						RepositoryOperationError: RepositoryOperationError.EntityNotFound
					})
				{
					existsException = exception;
				}

				return new List<SymbolSlim>();
			});


			if (existsException != null)
			{
				_logger.Log(LogLevel.Error, existsException, "Failed to resolve ancestors");
				return new Result<SymbolStatusInfo>(
					new RepositoryException("Failed to resolve ancestors."));
			}

			if (ancestors.Count == 0)
			{
				return statusInfo with { Version = "1", PreviousVersion = null };
			}

			var parent = ancestors.OrderByDescending(a => a.DateIssued).First();

			if (!int.TryParse(parent.Version, out var prevVersion))
			{
				return new Result<SymbolStatusInfo>(
					new RepositoryException("Failed to parse existing Version from db."));
			}

			var newVersion = prevVersion + 1;

			return statusInfo with { Version = newVersion.ToString(), PreviousVersion = parent.Id };

		};
	}

	public TryAsync<Unit> PublishToCommonLibAsync(string id, EngineeringSymbolStatusDto statusDto)
	{
		return new TryAsync<Unit>(async () =>
		{
			if (statusDto.Status == "Issued")
			{

				var sym = await _repo.GetEngineeringSymbolByIdAsync(id, true).Try();
				var symString = sym.Match(Succ: (s) => s, Fail: (exception) => "");

				//The following string shenanigans is a silly hack. A proper rebasing of the URL's should be done asap when new maintainers have been identified.
				symString = symString.Replace("https://rdf.equinor.com/ontology/engineering-symbol/v1#", "http://example.equinor.com/symbol#");

				try
				{

					var postres = await _downstreamApi.CallApiForAppAsync("CommonLib", options =>
					{
						options.HttpMethod = HttpMethod.Post;
						options.RelativePath = $"/api/symbol/WriteEngineeringSymbol";

						options.CustomizeHttpRequestMessage = message =>
						{
							message.Content = new StringContent(symString, System.Text.Encoding.UTF8, "application/json-patch+json");
						};
					});
					if (postres.IsSuccessStatusCode)
					{
						_logger.LogInformation($"Symbol with id: {id} posted to CL with successCode: {postres.StatusCode}");
					}
					else
					{
						_logger.LogError($"Symbol with id: {id} NOT posted to CL with errorcode: {postres.StatusCode}");
					}
				}
				catch (Exception ex)
				{
					_logger.LogError($"Posting to CL failed. {ex.Message}");
					return Unit.Default;
				}
				// We dont stop a run if this fails, return Unit.Default in all cases.
			}
			return Unit.Default;

		});
	}

	public TryAsync<Unit> DeleteSymbolAsync(string id)
	{
		return new Try<string>(() =>
			{
				var idValidator = new EngineeringSymbolIdValidator();

				if (!idValidator.Validate(id).IsValid)
					return new Result<string>(new ValidationException(new Dictionary<string, string[]>
					{
						{"id", new[] {"Invalid symbol Id"}}
					}));

				return id;
			})
			.ToAsync()
			.Bind(_repo.DeleteEngineeringSymbolAsync);
	}
}