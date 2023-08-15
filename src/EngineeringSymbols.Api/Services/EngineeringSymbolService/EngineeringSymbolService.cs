using System.Security.Claims;
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
    
    public EngineeringSymbolService(IConfiguration config, IEngineeringSymbolRepository repo, ILoggerFactory loggerFactory)
    {
        _repo = repo;
        _logger = loggerFactory.CreateLogger("EngineeringSymbolService");
    }

    public TryAsync<IEnumerable<EngineeringSymbolDto>> GetSymbolsAsync(bool allVersions = false)
        => _repo.GetAllEngineeringSymbolsAsync(distinct: !allVersions)
            .Map(symbols => symbols.Map(symbol => symbol.ToDto()));
    
    public TryAsync<IEnumerable<EngineeringSymbolPublicDto>> GetSymbolsPublicAsync(bool allVersions = false) 
        => _repo.GetAllEngineeringSymbolsAsync(distinct: !allVersions)
            .Map(symbols => symbols.Map(symbol => symbol.ToPublicDto()));
    
    
    public TryAsync<EngineeringSymbolDto> GetSymbolByIdOrKeyAsync(string idOrKey) 
        => _GetSymbolByIdOrKeyPublicAsync(idOrKey).Map(symbol => symbol.ToDto());
    
    
    public TryAsync<EngineeringSymbolPublicDto> GetSymbolByIdOrKeyPublicAsync(string idOrKey)
        => _GetSymbolByIdOrKeyPublicAsync(idOrKey).Map(symbol => symbol.ToPublicDto());
    
    private TryAsync<EngineeringSymbol> _GetSymbolByIdOrKeyPublicAsync(string idOrKey) =>
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
            EngineeringSymbolValidation.ValidateKey(idOrKey)
                .IfSuccess(validKey => { idAsKey = validKey; });
        }
        
        if (idAsGuid != null)
        {
            return await _repo.GetEngineeringSymbolByIdAsync(idAsGuid).Try();
        }
        
        if(idAsKey != null)
        {
            return await _repo.GetEngineeringSymbolByKeyAsync(idAsKey).Try();
        }
  
        return new Result<EngineeringSymbol>(new ValidationException(new Dictionary<string, string[]>
        {
            {"idOrKey", new [] { "Provided symbol identifier 'idOrKey' is not a valid GUID or Symbol Key"}}
        }));
    };


    
    public TryAsync<string> CreateSymbolFromFileAsync(ClaimsPrincipal user, IFormFile svgFile) =>
        async () => await CreateInsertContextFromFile(user, svgFile)
            .Bind(ReadFileToString)
            .Bind(ParseSvgString)
            .Bind(CreateInsertDto)
            .MapAsync(async ctx => await _repo.InsertEngineeringSymbolAsync(ctx.EngineeringSymbolDto).Try())
            .IfFail(exception => new Result<string>(exception));

    
    public TryAsync<string> CreateSymbolFromJsonAsync(ClaimsPrincipal user, EngineeringSymbolCreateDto createDto) =>
        async () => await CreateInsertContextFromDto(user, createDto)
            .Bind(CreateInsertDto)
            .MapAsync(async ctx => await _repo.InsertEngineeringSymbolAsync(ctx.EngineeringSymbolDto).Try())
            .IfFail(exception => new Result<string>(exception));
    
    public TryAsync<bool> ReplaceSymbolAsync(EngineeringSymbolDto symbolDto) =>
        async () => new Result<bool>(false);
    //async () => await CreateUpdateContext(id, updateDto)
        //    .Bind(ValidateInput)
        //    .MapAsync(async ctx => await _repo.UpdateEngineeringSymbolAsync(ctx.Id, ctx.UpdateDto).Try())
        //    .IfFail(exception => new Result<bool>(exception));

    public TryAsync<bool> DeleteSymbolAsync(string id) =>
        async () =>
        {
            var idParsed = EngineeringSymbolValidation.ParseEngineeringSymbolId(id);
            return await _repo.DeleteEngineeringSymbolAsync(idParsed.ToString()).Try();
        };
}