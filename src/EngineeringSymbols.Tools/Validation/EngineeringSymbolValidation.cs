namespace EngineeringSymbols.Tools.Validation;

public static class EngineeringSymbolValidation
{
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
    
    public static Validation<ValidationError, Guid> ValidateId(string id)
    {
        return Guid.TryParse(id, out var result) 
            ? Success<ValidationError, Guid>(result) 
            : Fail<ValidationError, Guid>(new ValidationError("id",$"Not a valid GUID"));
    }
    
    public static Validation<ValidationError, string> ValidateKey(string key)
    {
        var errors = Seq<ValidationError>();
        //if (string.IsNullOrEmpty(key) || string.IsNullOrWhiteSpace(key))
        //    return Fail<Error, string>(Error.New($"'key' was null or empty"));
        
        if(key.Length is > 60 or < 4)
            errors = errors.Add(new ValidationError("key", "invalid lenght (valid: 4-60)"));
        
        var legalChars = new [] {'-', '_'};
        
        if (ContainsIllegalChars(key, legalChars))
            errors = errors.Add(new ValidationError("key", "contains illegal characters"));
        
        return errors.Length == 0
            ? Success<ValidationError, string>(key) 
            : Fail<ValidationError, string>(errors);
    }
    
    public static Validation<ValidationError, string> ValidateOwner(string owner)
    {
        var errors = Seq<ValidationError>();
        //if (string.IsNullOrEmpty(owner) || string.IsNullOrWhiteSpace(owner))
        //    return Fail<Error, string>(Error.New($"'owner' was null or empty"));
        
        if(owner.Length is > 120 or < 5)
            errors = errors.Add(new ValidationError("owner", "invalid lenght (valid: 5-120)"));
        
        var legalChars = new [] {'-', '_', '@', '.'};

        if (ContainsIllegalChars(owner, legalChars))
            errors = errors.Add(new ValidationError("owner", "contains illegal characters"));
        
        return errors.Length == 0
            ? Success<ValidationError, string>(owner) 
            : Fail<ValidationError, string>(errors);
    }
    
    public static Validation<ValidationError, string> ValidateDescription(string description)
    {
        var errors = Seq<ValidationError>();

        if (description.Length is > 140 or < 2)
            errors = errors.Add(new ValidationError("description","invalid lenght (valid: 2-140)"));
        
        var legalChars = new [] {'-', '_', '@', '.', '!', '?'};

        if (ContainsIllegalChars(description, legalChars))
            errors = errors.Add(new ValidationError("description","contains illegal characters"));
        
        return errors.Length == 0
            ? Success<ValidationError, string>(description) 
            : Fail<ValidationError, string>(errors);
    }

    private static bool ContainsIllegalChars(string value, char[]? whitelist = null)
    {
        var wl = whitelist ?? new[] {'-', '_'};
        return !value.ToCharArray().All(c =>
            char.IsLetter(c)
            || char.IsNumber(c)
            || wl.Contains(c));
    }
    
}