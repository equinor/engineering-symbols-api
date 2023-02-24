using LanguageExt.SomeHelp;

namespace EngineeringSymbols.Api.Endpoints;

public static class GetEngineeringSymbols
{
    public static async Task<IResult> GetAllAsync(IEngineeringSymbolService symbolService, string? key, bool? full, bool? allVersions) =>
        await symbolService.GetSymbolsAsync()
            .Match(
                Succ: TypedResults.Ok,
                Fail: Common.OnFailure);

    public static async Task<IResult> GetByIdOrKeyAsync(string idOrKey, IEngineeringSymbolService symbolService) => 
        await symbolService.GetSymbolByIdOrKeyAsync(idOrKey)
            .Map(symbol => symbol.ToResponseDto())
            .Match(
                Succ: TypedResults.Ok,
                Fail: Common.OnFailure);
}