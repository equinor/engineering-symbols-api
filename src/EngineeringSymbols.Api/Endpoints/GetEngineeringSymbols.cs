using LanguageExt.SomeHelp;

namespace EngineeringSymbols.Api.Endpoints;

public static class GetEngineeringSymbols
{
    public static async Task<IResult> GetAllAsync(IEngineeringSymbolService symbolService) =>
        await symbolService.GetSymbolsAsync()
            .Match(
                Succ: TypedResults.Ok,
                Fail: Common.OnFailure);
    
    public static async Task<IResult> GetByIdAsync(string id, IEngineeringSymbolService symbolService) => 
        await symbolService.GetSymbolAsync(id)
            .Match(
                Succ: TypedResults.Ok,
                Fail: Common.OnFailure);
}