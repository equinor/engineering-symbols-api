using EngineeringSymbols.Api.Repositories;
using EngineeringSymbols.Tools;
using EngineeringSymbols.Tools.Constants;
using EngineeringSymbols.Tools.Entities;
using EngineeringSymbols.Tools.Models;
using EngineeringSymbols.Tools.Validation;
using Newtonsoft.Json.Linq;
using VDS.RDF.JsonLd;

namespace EngineeringSymbols.Api.Services;

public class EngineeringSymbolService : IEngineeringSymbolService
{
    private readonly IEngineeringSymbolRepository _repo;

    private readonly ILogger _logger;

    public EngineeringSymbolService(IConfiguration config, IEngineeringSymbolRepository repo,
        ILoggerFactory loggerFactory)
    {
        _repo = repo;
        _logger = loggerFactory.CreateLogger("EngineeringSymbolService");
    }

    private static Func<string, TryAsync<JObject>> AddFraming(bool publicVersion)
    {
        return jsonLdContent => () =>
        {
            var input = JToken.Parse(jsonLdContent);
            var frame = publicVersion ? SymbolFrames.FramePublic : SymbolFrames.FrameInternal;
            var framed = JsonLdProcessor.Frame(input, frame, new JsonLdProcessorOptions { Explicit = true });
            return Task.FromResult(new Result<JObject>(framed));
        };
    }
    
    public TryAsync<JObject> GetSymbolsAsync(bool onlyLatestVersion, bool publicVersion)
        => _repo.GetAllEngineeringSymbolsAsync(onlyLatestVersion, onlyPublished: publicVersion)
            .Bind(AddFraming(publicVersion));

    public TryAsync<JObject> GetSymbolByIdOrIdentifierAsync(string idOrIdentifier, bool publicVersion)
        => new Try<(string, bool)>(() =>
            {
                if (Guid.TryParse(idOrIdentifier, out _))
                {
                    return (idOrIdentifier, true);
                }

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
                ? _repo.GetEngineeringSymbolByIdAsync(idOrKeyTuple.Item1, onlyPublished: publicVersion)
                : _repo.GetEngineeringSymbolByIdentifierAsync(idOrKeyTuple.Item1, onlyPublished: publicVersion))
            .Bind(AddFraming(publicVersion));
    
    public TryAsync<EngineeringSymbol> CreateSymbolAsync(EngineeringSymbolCreateDto createDto, bool validationOnly)
        => new Try<EngineeringSymbolCreateDto>(
                () => createDto.Validate().ToEither()
                    .Match<Result<EngineeringSymbolCreateDto>>(
                        Right: dto => dto,
                        Left: errors => new Result<EngineeringSymbolCreateDto>(new ValidationException(errors))))
            .ToAsync()
            .Bind(ResolveVersion)
            .Bind(symbol => !validationOnly
                ? _repo.PutEngineeringSymbolAsync(symbol)
                : async () => symbol with {Id = "", DateTimeCreated = DateTimeOffset.UnixEpoch});


    private static SymbolSlim ToSymbolSlim(JObject obj)
    {
        // "dc:identifier": "PP007A_alt1",
        // "pav:version": "1",
        // "pav:previousVersion": "...",
        // "esmde:status": "Draft",

        var id = (string)obj["@id"];
        var identifier = (string) obj[EsProp.IdentifierQName];
        var version = (string)obj[EsProp.VersionQName];
        
        return new SymbolSlim(id, identifier, version);
    }
    
    private TryAsync<EngineeringSymbol> ResolveVersion(EngineeringSymbolCreateDto createDto)
        => async () =>
        {
            // Two paths:
            // 1: 'Identifier' exists
            //     - 'IsRevisionOf' must not be null and have an equal 'Identifier'
            // 2: 'Identifier' does not exist
            //     - 'IsRevisionOf' must be null or empty
            
            // Check if Identifier exists
            var existsResult = await _repo.GetEngineeringSymbolByIdentifierAsync(createDto.Identifier, onlyPublished: true)
                .Bind(AddFraming(publicVersion: false))
                .Try();
            
            Exception? existsException = null;
            
            // Check if multiple, single or none...
            var ancestors = existsResult.Match<List<SymbolSlim>>(jObject =>
            {
                var res = new List<SymbolSlim> ();
                
                // Top level symbol graph
                if (jObject.ContainsKey("@graph") && jObject.ContainsKey("@id"))
                {
                    res.Add(ToSymbolSlim(jObject));
                }
                else if (jObject.TryGetValue("@graph", out var value))
                {
                    res.AddRange(from g in value as JArray select ToSymbolSlim(g as JObject));
                }

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

                return new List<SymbolSlim> ();
            });
            
            var version = 1;

            if (ancestors.Count > 0)
            {
                if (createDto.IsRevisionOf == null)
                {
                    return new Result<EngineeringSymbol>(new ValidationException(
                        $"'{nameof(createDto.Identifier)}' '{createDto.Identifier}' already exists, but '{nameof(createDto.IsRevisionOf)}' is not provided."));
                }
                
                var parent = ancestors.Find(s => s.Id == createDto.IsRevisionOf);
                
                if (parent == null)
                {
                    return new Result<EngineeringSymbol>(new ValidationException(
                        $"'{nameof(createDto.IsRevisionOf)}' is invalid."));
                }
                
                if (!int.TryParse(parent.Version, out var parentVersion))
                {
                    return new Result<EngineeringSymbol>(new RepositoryException($"Failed to parse parent '{nameof(parent.Version)}'"));
                }

                version = parentVersion + 1;
            } 
            else if (createDto.IsRevisionOf != null)
            {
                return new Result<EngineeringSymbol>(new ValidationException($"Expected '{nameof(createDto.IsRevisionOf)}' to be null because there does not exist any symbol with '{nameof(createDto.Identifier)}' = '{createDto.Identifier}' in the database."));
            }
            
            return createDto.ToInsertEntity() with
            {
                Version = version.ToString(), 
                PreviousVersion = createDto.IsRevisionOf
            };
        };
            
            
    /*public TryAsync<bool> UpdateSymbolAsync(string id, EngineeringSymbolCreateDto createDto)
        => GetSymbolByIdOrKeyAsync(id, publicVersion: false)
            .Map(symbols =>
            {
                var symbol = symbols.ToArray().First();
                
                return createDto.ToDto() with
                {
                    Id = symbol.Id,
                    DateTimeCreated = symbol.DateTimeCreated,
                    DateTimePublished = symbol.DateTimePublished,
                    Status = symbol.Status
                };
            })
            .Bind(dto => new TryAsync<EngineeringSymbolDto>(async () =>
            {
                if (dto.Status == EngineeringSymbolStatus.Published.ToString())
                {
                    return new Result<EngineeringSymbolDto>(
                        new ValidationException("Symbol has already been published."));
                }
                return dto;
            }))
            .Bind(_repo.ReplaceEngineeringSymbolAsync);

    public TryAsync<bool> UpdateSymbolStatusAsync(string id, EngineeringSymbolStatusDto statusDto)
        => GetSymbolByIdOrKeyAsync(id, publicVersion: false)
            .Bind(symbols => new TryAsync<EngineeringSymbolDto>(
                async () =>
                {
                    if (!Enum.TryParse<EngineeringSymbolStatus>(statusDto.Status, out var status) ||
                        status == EngineeringSymbolStatus.None)
                    {
                        var statusValues = Enum.GetValues(typeof(EngineeringSymbolStatus))
                            .OfType<EngineeringSymbolStatus>()
                            .Select(e => e.ToString()).ToList();

                        var validValues = string.Join(", ",
                            statusValues.Where(s => s != "None").Select(s => $"'{s}'"));

                        return new Result<EngineeringSymbolDto>(
                            new ValidationException(new Dictionary<string, string[]>
                            {
                                {nameof(statusDto.Status), new[] {$"Invalid value. Valid values are: {validValues}."}},
                            }));
                    }

                    var symbol = (EngineeringSymbolDto) symbols.ToArray().First();

                    var es = symbol.ToEngineeringSymbol();

                    if (es.Status == EngineeringSymbolStatus.Published)
                    {
                        return new Result<EngineeringSymbolDto>(
                            new ValidationException("Symbol has already been published."));
                    }

                    if (status == EngineeringSymbolStatus.Published)
                    {
                        return symbol with
                        {
                            DateTimePublished = DateTimeOffset.Now,
                            Status = EngineeringSymbolStatus.Published.ToString()
                        };
                    }

                    return symbol with {Status = status.ToString()};
                }))
            .Bind(_repo.ReplaceEngineeringSymbolAsync);*/


    public TryAsync<bool> DeleteSymbolAsync(string id)
        => new Try<string>(() =>
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