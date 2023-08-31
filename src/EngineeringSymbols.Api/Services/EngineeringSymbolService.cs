using EngineeringSymbols.Api.Repositories;
using EngineeringSymbols.Tools;
using EngineeringSymbols.Tools.Models;
using EngineeringSymbols.Tools.Validation;

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

    public TryAsync<IEnumerable<EngineeringSymbolResponse>> GetSymbolsAsync(bool onlyLatestVersion, bool onlyPublished)
        => _repo.GetAllEngineeringSymbolsAsync(onlyLatestVersion, onlyPublished)
            .Map(symbols => symbols.Map(symbol => onlyPublished
                ? (EngineeringSymbolResponse) symbol.ToPublicDto()
                : symbol.ToDto()));

    public TryAsync<IEnumerable<EngineeringSymbolResponse>> GetSymbolByIdOrKeyAsync(string idOrKey, bool publicVersion)
        => new Try<(string, bool)>(() =>
            {
                if (Guid.TryParse(idOrKey, out _))
                {
                    return (idOrKey, true);
                }

                var keyValidator = new EngineeringSymbolKeyValidator();

                return keyValidator.Validate(idOrKey).IsValid
                    ? (idOrKey, false)
                    : new Result<(string, bool)>(new ValidationException(
                        new Dictionary<string, string[]>
                            {{"idOrKey", new[] {"Value is not a valid GUID or Engineering Symbol Key."}}}));
            })
            .ToAsync()
            .Bind(idOrKeyTuple => idOrKeyTuple.Item2
                ? _repo.GetEngineeringSymbolByIdAsync(idOrKeyTuple.Item1, publicVersion)
                : _repo.GetEngineeringSymbolByKeyAsync(idOrKeyTuple.Item1, publicVersion))
            .Map(symbols => publicVersion
                ? (IEnumerable<EngineeringSymbolResponse>) symbols.Map(s => s.ToPublicDto())
                : symbols.Map(s => s.ToDto()));


    public TryAsync<EngineeringSymbolDto> CreateSymbolAsync(EngineeringSymbolCreateDto createDto, bool validationOnly)
        => new Try<EngineeringSymbolCreateDto>(
                () => createDto.Validate().ToEither()
                    .Match<Result<EngineeringSymbolCreateDto>>(
                        Right: dto => dto,
                        Left: errors => new Result<EngineeringSymbolCreateDto>(new ValidationException(errors))))
            .ToAsync()
            .Bind(dto => !validationOnly
                ? _repo.InsertEngineeringSymbolAsync(dto)
                : async () => dto.ToInsertEntity() with { Id = "", DateTimeCreated = DateTimeOffset.UnixEpoch})
            .Map(es => es.ToDto());


    public TryAsync<bool> UpdateSymbolAsync(string id, EngineeringSymbolCreateDto createDto)
        => GetSymbolByIdOrKeyAsync(id, publicVersion: false)
            .Map(symbols =>
            {
                var symbol = (EngineeringSymbolDto) symbols.ToArray().First();
                
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
            .Bind(_repo.ReplaceEngineeringSymbolAsync);


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