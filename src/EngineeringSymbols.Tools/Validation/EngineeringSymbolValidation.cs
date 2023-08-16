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

    public static Guid ParseEngineeringSymbolId(string id)
    {
        if (!Guid.TryParse(id, out var result))
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                {"id", new [] { "Invalid symbol Id"}}
            });
        }

        return result;
    }
    

    public static Validation<ValidationError, EngineeringSymbol> Validate(this EngineeringSymbolDto symbol)
    {
        var validator = new EngineeringSymbolDtoValidator();

        var result = validator.Validate(symbol);

        if (result.IsValid)
        {
            return Success<ValidationError, EngineeringSymbol>(symbol.ToEngineeringSymbol());
        }
        
        var errors = result.Errors.
            Select(e => new ValidationError(e.PropertyName, e.ErrorMessage))
            .ToSeq();
        
        return Fail<ValidationError, EngineeringSymbol>(errors);
    }
    
    public static Validation<ValidationError, EngineeringSymbolCreateDto> Validate(this EngineeringSymbolCreateDto symbol)
    {
        var validator = new EngineeringSymbolCreateDtoValidator();

        var result = validator.Validate(symbol);

        if (result.IsValid)
        {
            return Success<ValidationError, EngineeringSymbolCreateDto>(symbol);
        }
        
        var errors = result.Errors.
            Select(e => new ValidationError(e.PropertyName, e.ErrorMessage))
            .ToSeq();
        
        return Fail<ValidationError, EngineeringSymbolCreateDto>(errors);
    }

    public static Validation<ValidationError, string> ValidateKey(string? key)
    {
        var errors = Seq<ValidationError>();
        
        if (key == null ||string.IsNullOrEmpty(key) || string.IsNullOrWhiteSpace(key))
        {
            errors = errors.Add(new ValidationError("key","null or empty"));
            return Fail<ValidationError, string>(errors);
        }

        if(key is {Length: > 60 or < 4})
            errors = errors.Add(new ValidationError("key", "invalid lenght (valid: 4-60)"));
        
        var legalChars = new [] {'-', '_'};
        
        if (ContainsIllegalChars(key, legalChars))
            errors = errors.Add(new ValidationError("key", "contains illegal characters"));
        
        return errors.Length == 0
            ? Success<ValidationError, string>(key) 
            : Fail<ValidationError, string>(errors);
    }
}