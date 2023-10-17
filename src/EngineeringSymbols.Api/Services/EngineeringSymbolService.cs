using System.Globalization;
using EngineeringSymbols.Api.Repositories;
using EngineeringSymbols.Tools;
using EngineeringSymbols.Tools.Constants;
using EngineeringSymbols.Tools.Entities;
using EngineeringSymbols.Tools.Models;
using EngineeringSymbols.Tools.Rdf;
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
            var input = JToken.Parse(jsonLdContent, new JsonLoadSettings() { });
            var frame = publicVersion ? SymbolFrames.FramePublic : SymbolFrames.FrameInternal;
            var framed = JsonLdProcessor.Frame(input, frame, new JsonLdProcessorOptions {Explicit = true});
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
        return ResolveNewVersion(putDto)
            .Bind(symbol => !validationOnly
                ? _repo.PutEngineeringSymbolAsync(symbol)
                : async () => symbol with {Id = "", DateTimeCreated = DateTime.UnixEpoch});
    }

    private TryAsync<EngineeringSymbolPutDto> ValidatePutDto(EngineeringSymbolPutDto putDto)
    {
        return async () => putDto.Validate().ToEither()
            .Match<Result<EngineeringSymbolPutDto>>(
                dto => dto,
                errors => new Result<EngineeringSymbolPutDto>(new ValidationException(errors)));
    }


    private static SymbolSlim ToSymbolSlim(JObject obj)
    {
        // "dc:identifier": "PP007A_alt1",
        // "pav:version": "1",
        // "pav:previousVersion": "...",
        // "esmde:status": "Draft",

        var id = (string) obj["@id"];
        var identifier = (string) obj[EsProp.IdentifierQName];
        var version = (string) obj[EsProp.VersionQName];

        return new SymbolSlim(id, identifier, version);
    }

    private TryAsync<EngineeringSymbol> ResolveNewVersion(EngineeringSymbolPutDto putDto)
    {
        return async () =>
        {
            /*
             Two paths:
                1: 'Identifier' exists
                    - 'IsRevisionOf' must not be null
                    - 'new Identifier' must be equal to the 'current Identifier' in the 'IsRevisionOf' graph
                2: 'Identifier' does not exist
                    - 'IsRevisionOf' must be null or empty (to avoid confusion)
            */

            // Check if Identifier exists
            var existsResult = await _repo.GetEngineeringSymbolByIdentifierAsync(putDto.Identifier, true)
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
                    existsException = exception;

                return new List<SymbolSlim>();
            });

            var version = 1;

            if (ancestors.Count > 0)
            {
                if (string.IsNullOrEmpty(putDto.IsRevisionOf))
                    return new Result<EngineeringSymbol>(new ValidationException(
                        $"'{nameof(putDto.Identifier)}' '{putDto.Identifier}' already exists, but '{nameof(putDto.IsRevisionOf)}' is not provided."));

                var parent = ancestors.Find(s => s.Id == putDto.IsRevisionOf);

                if (parent == null)
                    return new Result<EngineeringSymbol>(new ValidationException(
                        $"'{nameof(putDto.IsRevisionOf)}' is invalid."));

                if (!int.TryParse(parent.Version, out var parentVersion))
                    return new Result<EngineeringSymbol>(
                        new RepositoryException($"Failed to parse parent '{nameof(parent.Version)}'"));

                version = parentVersion + 1;
            }
            else if (!string.IsNullOrEmpty(putDto.IsRevisionOf))
            {
                return new Result<EngineeringSymbol>(new ValidationException(
                    $"Expected '{nameof(putDto.IsRevisionOf)}' to be null because there does not exist any Issued symbols with '{nameof(putDto.Identifier)}' = '{putDto.Identifier}' in the database."));
            }

            return putDto.ToInsertEntity() with
            {
                Version = version.ToString(),
                PreviousVersion = putDto.IsRevisionOf
            };
        };
    }


    public TryAsync<EngineeringSymbol> UpdateSymbolAsync(string id, EngineeringSymbolPutDto putDto)
    {
        return _repo.GetEngineeringSymbolByIdAsync(id, false)
            .Bind(AddFraming(false))
            .Bind(existing => new TryAsync<EngineeringSymbol>(async () =>
                {
                    /*
                     * TODO: It hurts a bit to parse these fields in this method, should make
                     * TODO: a method to parse a JObject to an EngineeringSymbol
                     * */
                    
                    if (!existing.ContainsKey("@id"))
                        return new Result<EngineeringSymbol>(
                            new ValidationException("Expected root level '@id' field."));

                    if (!Enum.TryParse<EngineeringSymbolStatus>((string?) existing.GetValue(EsProp.EditorStatusQName),
                            out var currentStatus))
                        return new Result<EngineeringSymbol>(
                            new ValidationException(
                                $"Failed to read/parse '{EsProp.EditorStatusQName}' from existing state."));

                    if (currentStatus == EngineeringSymbolStatus.Issued)
                        return new Result<EngineeringSymbol>(
                            new ValidationException(
                                $"Cannot modify symbol because it has status '{EngineeringSymbolStatus.Issued}'."));

                    var currentIdentifier = (string?) existing.GetValue(EsProp.IdentifierQName);
                    if (currentIdentifier == null)
                        return new Result<EngineeringSymbol>(
                            new ValidationException(
                                $"Failed to read/parse '{EsProp.IdentifierQName}' from existing state."));

                    var currentVersion = (string?) existing.GetValue(EsProp.VersionQName);
                    if (currentVersion == null)
                        return new Result<EngineeringSymbol>(
                            new ValidationException(
                                $"Failed to read/parse '{EsProp.VersionQName}' from existing state."));

                    var currentPrevVersionObj = (JObject?) existing.GetValue(EsProp.PreviousVersionQName);
                    var currentPrevVersion = (string?)currentPrevVersionObj?.GetValue("@id");
                    
                    
                    // if (currentPrevVersion == null)
                    //     return new Result<EngineeringSymbol>(
                    //         new ValidationException(
                    //             $"Failed to read/parse '{EsProp.PreviousVersionQName}' from existing state."));
                    
                   

                    var createdDateToken = existing.GetValue(EsProp.DateCreatedQName);
                    
                    if (createdDateToken == null)
                    {
                        return new Result<EngineeringSymbol>(
                            new ValidationException(
                                $"Failed to read/parse '{EsProp.DateCreatedQName}' from existing state."));
                    }
                    
                    var createdDate = (DateTime)createdDateToken;

                    // These fields must be set by backend, other fields come from the putDto:  
                    //  - Id  
                    //  - Status => Dont update, just pass over (status is set by own endpoint)  
                    //  - Version + Prev version => Update if Identifier has changed  
                    //  - Dates...  
                    //  - UserIdentifier => added earlier in pipeline  


                    // "pav:version": "1",  
                    // "rdfs:label": "Pump PP007A",  
                    // "esmde:id": "4d193516-dbaf-4829-afd9-e5f327bc2dc6",  
                    // "esmde:oid": "d5327a96-0771-4c0c-9334-bd14a0d3cb09",  
                    // "esmde:status": "Draft",  

                    if (currentIdentifier != putDto.Identifier)
                        /*
                            Same rules as when creating a new symbol. If the identifier has been modified,
                            then Version and PreviousVersion must also update based on IsRevisionOf from dto
                            
                            Two paths:        
                            1: 'Identifier' exists
                                - 'IsRevisionOf' must not be null
                                - 'new Identifier' must be equal to the 'current Identifier' in the 'IsRevisionOf' graph
                            2: 'Identifier' does not exist
                                - 'IsRevisionOf' must be null or empty (to avoid confusion)
                        */
                        return await ResolveNewVersion(putDto).Map(es => es with
                        {
                            Id = id,
                            Status = currentStatus,
                            DateTimeCreated = createdDate,
                            DateTimeModified = DateTime.UtcNow
                        }).Try();

                    return putDto.ToInsertEntity() with
                    {
                        Id = id,
                        Status = currentStatus,
                        DateTimeCreated = createdDate,
                        DateTimeModified = DateTime.UtcNow,
                        Version = currentVersion,
                        PreviousVersion = currentPrevVersion
                    };
                }
            )).Bind(_repo.PutEngineeringSymbolAsync);
    }

    /*GetSymbolByIdOrIdentifierAsync(id, publicVersion: false)
            .Map(symbols =>
            {
                var symbol = symbols.ToArray().First();

                return putDto.ToDto() with
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
            .Bind(_repo.PutEngineeringSymbolAsync);*/


    public TryAsync<bool> UpdateSymbolStatusAsync(string id, EngineeringSymbolStatusDto statusDto)
        => _repo.GetEngineeringSymbolByIdAsync(id, false)
            .Bind(AddFraming(false))
            //.Bind(obj => SymbolGraphHelper.ToEngineeringSymbols(obj).ToAsync())
            .Bind(sObj => new TryAsync<(string, string)>(
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

                        return new Result<(string, string)>(
                            new ValidationException(new Dictionary<string, string[]>
                            {
                                {nameof(statusDto.Status), new[] {$"Invalid value. Valid values are: {validValues}."}},
                            }));
                    }

                    //var symbol = (EngineeringSymbolDto) symbols.ToArray().First(); 

                    var statusDb = (string?)sObj.GetValue(EsProp.EditorStatusQName);

                    if (!Enum.TryParse<EngineeringSymbolStatus>(statusDb, out var currentStatus))
                    {
                        return new Result<(string, string)>(new ValidationException("Failed to get existing status from db."));
                    }

                    if (currentStatus == EngineeringSymbolStatus.Issued)
                    {
                        return new Result<(string, string)>(
                            new ValidationException("The symbol has already been issued."));
                    }
                    
                    return (id, newStatus.ToString());

                    /*var es = symbol.ToEngineeringSymbol();

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

                    return symbol with {Status = status.ToString()};*/
                }))
            .Bind( value => _repo.UpdateSymbolStatusAsync(value.Item1, value.Item2))
            //.Bind(_repo.PutEngineeringSymbolAsync)
            .Map(s => true);


    public TryAsync<bool> DeleteSymbolAsync(string id)
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