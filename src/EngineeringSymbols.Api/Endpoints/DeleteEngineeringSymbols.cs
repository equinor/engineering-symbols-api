using LanguageExt.SomeHelp;

namespace EngineeringSymbols.Api.Endpoints;

public static class DeleteEngineeringSymbols
{
    public static async Task<IResult> DeleteSingleAsync(string id, IEngineeringSymbolService symbolService) =>
        await symbolService.DeleteSymbolAsync(id)
            .Match(
                Succ: b => TypedResults.NoContent(),
                Fail: Common.OnFailure);
}