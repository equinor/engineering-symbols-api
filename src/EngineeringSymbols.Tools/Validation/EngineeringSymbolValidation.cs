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
    

    public static Validation<ValidationError, EngineeringSymbol> Validate(EngineeringSymbolDto symbol)
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

    /*
    public static Validation<ValidationError, EngineeringSymbol> Validate(EngineeringSymbolDto symbol)
    { 
        var idV = ValidateId(symbol.Id);
        var keyV = ValidateKey(symbol.Key);
        var ownerV = ValidateOwner(symbol.Owner);
        var statusV = ValidateStatus(symbol.Status);
        var descV = ValidateDescription(symbol.Description);

        var dateCreatedV = ValidateDateTimeCreated(symbol.DateTimeCreated);
        var datePublishedV = ValidateDateTimeGreaterThan("DateTimePublished", symbol.DateTimePublished, "DateTimeCreated", symbol.DateTimeCreated);
        var dateUpdatedV = ValidateDateTimeGreaterThan("DateTimeUpdated", symbol.DateTimePublished, "DateTimeCreated", symbol.DateTimeCreated);
        
        
        return (idV, keyV, statusV, descV, dateCreatedV, dateUpdatedV, datePublishedV, ownerV, filenameV, geometryV, widthV, heightV, connectorsV).Apply(
            (id , key, status, desc, dateCreated, dateUpdated, datePublished, owner, filename, geometry, width, height, connectors) =>
                new EngineeringSymbol(
                    Id: id,
                    Key: key,
                    Status: status,
                    Description: desc,    
                    DateTimeCreated: dateCreated,
                    DateTimeUpdated: dateUpdated,
                    DateTimePublished: datePublished,
                    Owner: owner,
                    Filename: filename,
                    Geometry: geometry,
                    Width: width,
                    Height: height,
                    Connectors: connectors));
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
    
    public static Validation<ValidationError, string> ValidateId(string? id)
    {
        return Guid.TryParse(id, out var result) 
            ? Success<ValidationError, string>(result.ToString()) 
            : Fail<ValidationError, string>(new ValidationError("id",$"Not a valid GUID"));
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
    
    public static Validation<ValidationError, string> ValidateOwner(string? owner)
    {
        var errors = Seq<ValidationError>();
        
        if (owner == null ||string.IsNullOrEmpty(owner) || string.IsNullOrWhiteSpace(owner))
        {
            errors = errors.Add(new ValidationError("owner","null or empty"));
            return Fail<ValidationError, string>(errors);
        }

        if(owner is {Length: > 120 or < 5})
            errors = errors.Add(new ValidationError("owner", "invalid lenght (valid: 5-120)"));
        
        var legalChars = new [] {'-', '_', '@', '.'};

        if (ContainsIllegalChars(owner, legalChars))
            errors = errors.Add(new ValidationError("owner", "contains illegal characters"));
        
        return errors.Length == 0
            ? Success<ValidationError, string>(owner) 
            : Fail<ValidationError, string>(errors);
    }
    
    public static Validation<ValidationError, string> ValidateDescription(string? description)
    {
        var errors = Seq<ValidationError>();

        if (description == null)
        {
            errors = errors.Add(new ValidationError("description","invalid lenght (valid: 2-140)"));
            return Fail<ValidationError, string>(errors);
        }
        
        if (description is {Length: > 140 or < 2})
            errors = errors.Add(new ValidationError("description","invalid lenght (valid: 2-140)"));
        
        var legalChars = new [] {'-', '_', '@', '.', '!', '?'};

        if (ContainsIllegalChars(description, legalChars))
            errors = errors.Add(new ValidationError("description","contains illegal characters"));
        
        return errors.Length == 0
            ? Success<ValidationError, string>(description) 
            : Fail<ValidationError, string>(errors);
    }

    
    public static Validation<ValidationError, EngineeringSymbolStatus> ValidateStatus(string? status)
    {
        return Enum.TryParse(status, out EngineeringSymbolStatus result)
            ? Success<ValidationError, EngineeringSymbolStatus>(result) 
            : Fail<ValidationError, EngineeringSymbolStatus>(new ValidationError("status",$"Not a valid Status"));
    }
    
    public static Validation<ValidationError, DateTimeOffset> ValidateDateTimeCreated(DateTimeOffset? dateTimeCreated)
    {
        if (dateTimeCreated == null)
        {
            return Fail<ValidationError, DateTimeOffset>(new ValidationError("dateTimeCreated",$"was null"));
        }
        
        var res = DateTimeOffset.FromUnixTimeMilliseconds(dateTimeCreated.Value.ToUnixTimeMilliseconds());
        
        return Success<ValidationError, DateTimeOffset>(res);
    }
    
    public static Validation<ValidationError, DateTimeOffset> ValidateDateTimeGreaterThan(string field1, DateTimeOffset? field1Value,  string field2, DateTimeOffset? field2Value)
    {
        if (field1Value == null)
        {
            return Fail<ValidationError, DateTimeOffset>(new ValidationError(field1,$"was null"));
        }

        if (field2Value == null)
        {
            return Fail<ValidationError, DateTimeOffset>(new ValidationError(field2,$"was null"));
        }

        var res = DateTimeOffset.FromUnixTimeMilliseconds(field1Value.Value.ToUnixTimeMilliseconds());
        
        return field1Value > field2Value 
            ? Success<ValidationError, DateTimeOffset>(res) 
            : Fail<ValidationError, DateTimeOffset>(new ValidationError(field1,$"{field1} is less than {field2}"));
    }
    */
    

}