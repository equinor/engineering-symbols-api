using System.Security.Principal;
using EngineeringSymbols.Tools;
using EngineeringSymbols.Tools.Models;
using EngineeringSymbols.Tools.SvgParser;
using EngineeringSymbols.Tools.Validation;
using Newtonsoft.Json;

namespace EngineeringSymbols.Api.Services.EngineeringSymbolService;

public static class CreateSymbol
{
    public record InsertContext
    {
        public EngineeringSymbolCreateDto EngineeringSymbolCreateDto { get; init; }
        public EngineeringSymbolDto EngineeringSymbolDto { get; init; }
        public string CreatedUri { get; init; } = "UnknownUri";
    }

    public static TryAsync<InsertContext> CreateInsertContext(EngineeringSymbolCreateDto createDto) =>
        TryAsync(() => Task.FromResult(new InsertContext {EngineeringSymbolCreateDto = createDto, EngineeringSymbolDto = createDto.ToDto()}));

    public static TryAsync<InsertContext> ValidateEngineeringSymbol(InsertContext ctx) =>
        TryAsync(async () =>
        {
            ctx.EngineeringSymbolCreateDto.Validate()
                .IfFail(errors => throw new ValidationException(errors));

            return ctx;
        });
    
}