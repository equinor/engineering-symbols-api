using System.Security.Claims;
using EngineeringSymbols.Api.Repositories;
using EngineeringSymbols.Tools;
using EngineeringSymbols.Tools.Entities;
using EngineeringSymbols.Tools.Models;
using EngineeringSymbols.Tools.Validation;
using Lucene.Net.Util.Fst;
using static EngineeringSymbols.Api.Services.EngineeringSymbolService.CreateSymbol;


namespace EngineeringSymbols.Api.Services.EngineeringSymbolService;



public class EngineeringSymbolService : IEngineeringSymbolService
{
    private readonly IEngineeringSymbolRepository _repo;
    
    private readonly ILogger _logger;
    
    public EngineeringSymbolService(IConfiguration config, IEngineeringSymbolRepository repo, ILoggerFactory loggerFactory)
    {
        _repo = repo;
        _logger = loggerFactory.CreateLogger("EngineeringSymbolService");
    }

    public TryAsync<IEnumerable<IEngineeringSymbolResponse>> GetSymbolsAsync(bool allVersions = false, bool publicVersion = true)
        => _repo.GetAllEngineeringSymbolsAsync(distinct: !allVersions, onlyPublished: publicVersion)
            .Map(symbols => symbols.Map(
                symbol => publicVersion ? (IEngineeringSymbolResponse)symbol.ToPublicDto() : symbol.ToDto()));

    public TryAsync<IEngineeringSymbolResponse> GetSymbolByIdOrKeyAsync(string idOrKey, bool publicVersion = true) => 
        async () =>
        {
            string? idAsGuid = null;
            string? idAsKey = null;
        
            if(Guid.TryParse(idOrKey, out var parsedIdGuid))
            {
                idAsGuid = parsedIdGuid.ToString();
            } 
            else
            {
                var keyValidator = new EngineeringSymbolKeyValidator();
                if (keyValidator.Validate(idOrKey).IsValid)
                {
                    idAsKey = idOrKey;
                }
            }

            Result<EngineeringSymbol> result = default;
            
            if (idAsGuid != null)
            {
                result = await _repo.GetEngineeringSymbolByIdAsync(idAsGuid, onlyPublished: publicVersion).Try();
            } 
            else if(idAsKey != null)
            {
                result = await _repo.GetEngineeringSymbolByKeyAsync(idAsKey, onlyPublished: publicVersion).Try();
            }

            if (result == default)
            {
                return new Result<IEngineeringSymbolResponse>(new ValidationException(new Dictionary<string, string[]>
                {
                    {"idOrKey", new [] { "Provided symbol identifier 'idOrKey' is not a valid GUID or Symbol Key"}}
                }));
            }
            
            return result.Map(symbol =>
                publicVersion ? (IEngineeringSymbolResponse) symbol.ToPublicDto() : symbol.ToDto());
        };
    

    public TryAsync<string> CreateSymbolAsync(ClaimsPrincipal user, InsertContentType contentType, string content, bool validationOnly) =>
        async () => await CreateInsertContext(user, contentType, content, validationOnly)
            .Bind(ParseContent)
            .Bind(CreateInsertDto)
            .Bind(ValidateEngineeringSymbol)
            .MapAsync(async ctx => 
                validationOnly == false 
                    ? await _repo.InsertEngineeringSymbolAsync(ctx.EngineeringSymbolDto).Try() 
                    : "")
            .IfFail(exception => new Result<string>(exception));
    
    
    public TryAsync<bool> ReplaceSymbolAsync(EngineeringSymbolCreateDto createDto) =>
        async () => new Result<bool>(false);
    //async () => await CreateUpdateContext(id, updateDto)
        //    .Bind(ValidateInput)
        //    .MapAsync(async ctx => await _repo.UpdateEngineeringSymbolAsync(ctx.Id, ctx.UpdateDto).Try())
        //    .IfFail(exception => new Result<bool>(exception));

    public TryAsync<bool> DeleteSymbolAsync(string id) =>
        async () =>
        {
            var idValidator = new EngineeringSymbolIdValidator();

            if (!idValidator.Validate(id).IsValid)
            {
                throw new ValidationException(new Dictionary<string, string[]>
                {
                    {"id", new [] { "Invalid symbol Id"}}
                });
            }

            return await _repo.DeleteEngineeringSymbolAsync(id).Try();
        };
}