using EngineeringSymbols.Api.Repositories;
using EngineeringSymbols.Tools;
using EngineeringSymbols.Tools.Entities;
using EngineeringSymbols.Tools.Models;
using EngineeringSymbols.Tools.Validation;
using static EngineeringSymbols.Api.Services.EngineeringSymbolService.CreateSymbol;


namespace EngineeringSymbols.Api.Services.EngineeringSymbolService;

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

    public TryAsync<IEnumerable<EngineeringSymbolResponse>> GetSymbolsAsync(bool allVersions = false,
        bool publicVersion = true)
    {
        return _repo.GetAllEngineeringSymbolsAsync(!allVersions, publicVersion)
            .Map(symbols => symbols.Map(
                symbol => publicVersion ? (EngineeringSymbolResponse) symbol.ToPublicDto() : symbol.ToDto()));
    }

    public TryAsync<IEnumerable<EngineeringSymbolResponse>> GetSymbolByIdOrKeyAsync(string idOrKey,
        bool publicVersion = true)
    {
        return async () =>
        {
            string? idAsGuid = null;
            string? idAsKey = null;

            if (Guid.TryParse(idOrKey, out var parsedIdGuid))
            {
                idAsGuid = parsedIdGuid.ToString();
            }
            else
            {
                var keyValidator = new EngineeringSymbolKeyValidator();
                if (keyValidator.Validate(idOrKey).IsValid) idAsKey = idOrKey;
            }

            Result<IEnumerable<EngineeringSymbol>> result = default;

            if (idAsGuid != null)
            {
                result = await _repo.GetEngineeringSymbolByIdAsync(idAsGuid, publicVersion).Try();
            }
            else if (idAsKey != null)
            {
                result = await _repo.GetEngineeringSymbolByKeyAsync(idAsKey, publicVersion).Try();
            }

            if (result == default)
                return new Result<IEnumerable<EngineeringSymbolResponse>>(new ValidationException(
                    new Dictionary<string, string[]>
                    {
                        {"idOrKey", new[] {"Provided symbol identifier 'idOrKey' is not a valid GUID or Symbol Key"}}
                    }));

            return result.Map(symbols =>
                publicVersion
                    ? (IEnumerable<EngineeringSymbolResponse>) symbols.Map(s => s.ToPublicDto())
                    : symbols.Map(s => s.ToDto()));
        };
    }


    public TryAsync<string> CreateSymbolAsync(EngineeringSymbolCreateDto createDto, bool validationOnly)
    {
        return async () => await CreateInsertContext(createDto)
            .Bind(ValidateEngineeringSymbol)
            .MapAsync(async ctx =>
                validationOnly == false
                    ? await _repo.InsertEngineeringSymbolAsync(ctx.EngineeringSymbolDto).Try()
                    : string.Empty)
            .IfFail(exception => new Result<string>(exception));
    }


    public TryAsync<bool> UpdateSymbolAsync(string id, EngineeringSymbolCreateDto createDto,
        bool isSuperAdmin = false)
    {
        return async () => await GetSymbolByIdOrKeyAsync(id, publicVersion: false)
            .Map(symbols =>
            {
                var symbol = (EngineeringSymbolDto) symbols.ToArray().First();
                
                return createDto.ToDto() with
                {
                    Id = symbol.Id,
                    DateTimeCreated = symbol.DateTimeCreated,
                    DateTimeUpdated = DateTimeOffset.Now,
                    DateTimePublished = symbol.DateTimePublished,
                    Status = symbol.Status
                };
            })
            .MapAsync(async dto => await _repo.ReplaceEngineeringSymbolAsync(dto).Try())
            .IfFail(exception => new Result<bool>(exception));
    }


    public TryAsync<bool> UpdateSymbolStatusAsync(string id)
    {
        throw new NotImplementedException();
    }

    public TryAsync<bool> DeleteSymbolAsync(string id)
    {
        return async () =>
        {
            var idValidator = new EngineeringSymbolIdValidator();

            if (!idValidator.Validate(id).IsValid)
                throw new ValidationException(new Dictionary<string, string[]>
                {
                    {"id", new[] {"Invalid symbol Id"}}
                });

            return await _repo.DeleteEngineeringSymbolAsync(id).Try();
        };
    }
}