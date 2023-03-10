using System.Security.Claims;
using EngineeringSymbols.Api.Repositories;
using EngineeringSymbols.Tools.Entities;
using EngineeringSymbols.Tools.Models;
using EngineeringSymbols.Tools.Validation;
using VDS.RDF.Shacl.Validation;
using static EngineeringSymbols.Api.Services.EngineeringSymbolService.CreateSymbol;
using static EngineeringSymbols.Api.Services.EngineeringSymbolService.UpdateSymbol;

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

    public TryAsync<IEnumerable<IEngineeringSymbolResponseDto>> GetSymbolsAsync(bool? allVersions = false, int? detailLevel = 0)
    {
        if (allVersions != null || allVersions == true)
        {
            return _repo.GetAllEngineeringSymbolsIncludeAllVersionsAsync();
        }
        
        return _repo.GetAllEngineeringSymbolsAsync();
    }

    public TryAsync<EngineeringSymbol> GetSymbolByIdOrKeyAsync(string idOrKey) => async () => 
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

    public TryAsync<string> CreateSymbolAsync(ClaimsPrincipal user, IFormFile svgFile) =>
        async () => await CreateInsertContext(user, svgFile)
            .Bind(ReadFileToString)
            .Bind(ParseSvgString)
            .MapAsync(async ctx => await _repo.InsertEngineeringSymbolAsync(ctx.EngineeringSymbolCreateDto!).Try())
            .IfFail(exception => new Result<string>(exception));

    public TryAsync<bool> UpdateSymbolAsync(string id, EngineeringSymbolUpdateDto updateDto) =>
        async () => await CreateUpdateContext(id, updateDto)
            .Bind(ValidateInput)
            .MapAsync(async ctx => await _repo.UpdateEngineeringSymbolAsync(ctx.Id, ctx.UpdateDto).Try())
            .IfFail(exception => new Result<bool>(exception));

    public TryAsync<bool> DeleteSymbolAsync(string id) =>
        async () =>
        {
            var idParsed = EngineeringSymbolValidation.ParseEngineeringSymbolId(id);
            return await _repo.DeleteEngineeringSymbolAsync(idParsed.ToString()).Try();
        };
}