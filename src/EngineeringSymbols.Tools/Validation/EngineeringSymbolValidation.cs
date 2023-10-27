using EngineeringSymbols.Tools.Entities;

namespace EngineeringSymbols.Tools.Validation;

public static class EngineeringSymbolValidation
{
    public static bool ContainsIllegalChars(string value, char[]? whitelist = null)
    {
        var wl = whitelist ?? new[] {'-', '_'};
        return !value.ToCharArray().All(c =>
            char.IsLetter(c)
            || char.IsNumber(c)
            || wl.Contains(c));
    }
    
    public static bool ContainsOnlyLatinLetters(string value)
    {
        return value.All(c => c is >= 'A' and <= 'Z' or >= 'a' and <= 'z');
    }
    
    public static Validation<ValidationError, EngineeringSymbolPutDto> Validate(this EngineeringSymbolPutDto symbol)
    {
        var validator = new EngineeringSymbolCreateDtoValidator();

        var result = validator.Validate(symbol);

        if (result.IsValid)
        {
            return Success<ValidationError, EngineeringSymbolPutDto>(symbol);
        }
        
        var errors = result.Errors.
            Select(e => new ValidationError(e.PropertyName, e.ErrorMessage))
            .ToSeq();
        
        return Fail<ValidationError, EngineeringSymbolPutDto>(errors);
    }
}