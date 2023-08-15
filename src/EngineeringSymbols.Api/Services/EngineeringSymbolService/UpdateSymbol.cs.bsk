using EngineeringSymbols.Tools.Models;
using EngineeringSymbols.Tools.Validation;

namespace EngineeringSymbols.Api.Services.EngineeringSymbolService;

public static class UpdateSymbol
{
    
    public record UpdateContext(string Id, EngineeringSymbolUpdateDto UpdateDto);
    
    public static TryAsync<UpdateContext> CreateUpdateContext(string id, EngineeringSymbolUpdateDto updateDto) =>
        TryAsync(() => Task.FromResult(new UpdateContext(id, updateDto)));
    
   public static TryAsync<UpdateContext> ValidateInput(UpdateContext ctx) =>
        TryAsync(() =>
        {
            var idV = EngineeringSymbolValidation.ValidateId(ctx.Id);

            // Validate fields. Because minimum one field is required, a null value is success.
            var keyV = ctx.UpdateDto.Key == null 
                ? Success<ValidationError, string>(string.Empty)
                : EngineeringSymbolValidation.ValidateKey(ctx.UpdateDto.Key);
            
            var ownerV = ctx.UpdateDto.Owner == null 
                ? Success<ValidationError, string>(string.Empty)
                : EngineeringSymbolValidation.ValidateOwner(ctx.UpdateDto.Owner);
            
            var descriptionV = ctx.UpdateDto.Description == null 
                ? Success<ValidationError, string>(string.Empty)
                : EngineeringSymbolValidation.ValidateDescription(ctx.UpdateDto.Description);

            // Everything cant be null
            var atLeastOneFieldV = ctx.UpdateDto.Key == null 
                            && ctx.UpdateDto.Owner == null
                            && ctx.UpdateDto.Description == null
                ? Fail<ValidationError, Unit>(new ValidationError("-","No values to update provided"))
                : Success<ValidationError, Unit>(new Unit());

            return (idV, keyV, ownerV, descriptionV, atLeastOneFieldV).Apply(
                (id, key, owner, description, _) => 
                   Task.FromResult(new UpdateContext(id.ToString(), 
                        new EngineeringSymbolUpdateDto
                        {
                            Key = key == string.Empty ? null : key,
                            Owner = owner == string.Empty ? null : owner,
                            Description = description == string.Empty ? null : description,
                        })))
                .IfFail(errors =>
                {
                    var errorDict = errors.Aggregate(new Dictionary<string, string[]>(), (acc, error) =>
                    {
                        var cat = error.Category ?? "-";
                        
                        if (acc.ContainsKey(cat))
                        {
                            acc[cat] = acc[cat].Concat(new[] {error.Value}).ToArray();
                        }
                        else
                        {
                            acc[cat] = new[] {error.Value};
                        }
                        return acc;
                    });
                    
                    throw new ValidationException(errorDict);
                });
        });
}