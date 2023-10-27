using EngineeringSymbols.Tools.Entities;
using FluentValidation;
using ICSharpCode.SharpZipLib.Core;
using VDS.RDF.Ontology;
using Ontology = EngineeringSymbols.Tools.Constants.Ontology;

namespace EngineeringSymbols.Tools.Validation;

public static class EngineeringSymbolFieldValidators
{
	public static readonly char[] TextWhiteList = { '-', '_', '.', ',', '!', '?', ' ' };

	public const int BaseSvgSize = 1;

	public static IRuleBuilderOptions<T, string> MustBeValidEngineeringSymbolId<T>(this IRuleBuilder<T, string> ruleBuilder)
	{
		return ruleBuilder
			.Must(s => Guid.TryParse(s, out _))
			.WithMessage("Is not a valid Engineering Symbol Id");
	}

	public static IRuleBuilderOptions<T, string> MustBeValidEngineeringSymbolIdentifier<T>(this IRuleBuilder<T, string> ruleBuilder)
	{
		return ruleBuilder
			.NotNull()
			.MinimumLength(4)
			.MaximumLength(64)
			.Must(s => !EngineeringSymbolValidation.ContainsIllegalChars(s, new[] { '-', '_' }))
			.WithMessage("Illegal characters detected. Only letters, numbers, '-' and '_' is allowed.");
	}

    
	public static IRuleBuilderOptions<T, string?> MustBeValidSymbolIri<T>(this IRuleBuilder<T, string?> ruleBuilder)
	{
		return ruleBuilder
			.Must(value =>
			{
				if (string.IsNullOrEmpty(value)) return false;
				if (!value.StartsWith(Ontology.SymbolIri)) return false;

				var id = value.Split('/').Last();
				
				if (!Guid.TryParse(id, out _)) return false;
				
				return true;
			}).WithMessage($"Value is not a valid Engineering Symbol IRI. Expected format: '{Ontology.SymbolIri}<GUID>'");
	}
	
    
	public static IRuleBuilderOptions<T, string> MustBeValidEngineeringSymbolLabel<T>(this IRuleBuilder<T, string> ruleBuilder)
	{
		return ruleBuilder
			.NotNull()
			.MaximumLength(250)
			.Must(s => !EngineeringSymbolValidation.ContainsIllegalChars(s, TextWhiteList))
			.WithMessage("Invalid characters");
	}
	
	public static IRuleBuilderOptions<T, string> MustBeValidEngineeringSymbolDescription<T>(this IRuleBuilder<T, string> ruleBuilder)
	{
		return ruleBuilder
			.NotNull()
			.MaximumLength(250)
			.Must(s => !EngineeringSymbolValidation.ContainsIllegalChars(s, TextWhiteList))
			.WithMessage("Invalid characters");
	}
    
	
	public static IRuleBuilderOptions<T, string> MustBeValidEngineeringSymbolSource<T>(this IRuleBuilder<T, string> ruleBuilder)
	{
		return ruleBuilder.NotNull().NotEmpty().MaximumLength(250);
	}
	
	public static IRuleBuilderOptions<T, string> MustBeValidEngineeringSymbolSubject<T>(this IRuleBuilder<T, string> ruleBuilder)
	{
		return ruleBuilder.NotNull().NotEmpty().MaximumLength(250);
	}
	
	public static IRuleBuilderOptions<T, User> MustBeValidEngineeringSymbolUser<T>(this IRuleBuilder<T, User> ruleBuilder)
	{
		return ruleBuilder
			.NotNull()
			.Must(u => !string.IsNullOrEmpty(u.Name))
			.WithMessage("User 'name' cannot be null or empty")
			.Must(u => !string.IsNullOrEmpty(u.Email))
			.WithMessage("User 'email' cannot be null or empty");
	}
	
	public static IRuleBuilderOptions<T, string> MustBeValidEngineeringSymbolColor<T>(this IRuleBuilder<T, string> ruleBuilder)
	{
		return ruleBuilder
			.NotEmpty()
			.MaximumLength(50);
	}
	
	
	
	public static IRuleBuilderOptions<T, DateTime> MustBeValidEngineeringSymbolDateCreated<T>(this IRuleBuilder<T, DateTime> ruleBuilder)
	{
		return ruleBuilder
			.NotNull()
			.Must(d => d != DateTime.UnixEpoch);
	}

	public static IRuleBuilderOptions<T, DateTime> MustBeValidEngineeringSymbolDateModified<T>(this IRuleBuilder<T, DateTime> ruleBuilder)
	{
		return ruleBuilder
			.NotNull();
	}

	public static IRuleBuilderOptions<T, DateTime> MustBeValidEngineeringSymbolDateIssued<T>(this IRuleBuilder<T, DateTime> ruleBuilder)
	{
		return ruleBuilder
			.NotNull();
	}

	public static IRuleBuilderOptions<T, int> MustBeValidEngineeringSymbolWidth<T>(this IRuleBuilder<T, int> ruleBuilder)
	{
		return ruleBuilder
			.NotNull()
			.GreaterThanOrEqualTo(BaseSvgSize)
			.WithMessage($"'width' must not be empty, or have a value less than {BaseSvgSize}.")
			.Must(w => w % BaseSvgSize == 0)
			.WithMessage($"'width' is not a multiple of {BaseSvgSize}");
	}

	public static IRuleBuilderOptions<T, int> MustBeValidEngineeringSymbolHeight<T>(this IRuleBuilder<T, int> ruleBuilder)
	{
		return ruleBuilder
			.NotNull()
			.GreaterThanOrEqualTo(BaseSvgSize)
			.WithMessage($"'height' must not be empty, or have a value less than {BaseSvgSize}.")
			.Must(h => h % BaseSvgSize == 0)
			.WithMessage($"'height' is not a multiple of {BaseSvgSize}");
	}


	public static IRuleBuilderOptions<T, List<ConnectionPoint>> MustBeValidEngineeringSymbolConnectionPointsList<T>(this IRuleBuilder<T, List<ConnectionPoint>> ruleBuilder)
	{
		return ruleBuilder
			.NotNull()
			.WithMessage("'connectionPoints' array must not be null");
	}

	public static IRuleBuilderOptions<T, string> MustBeValidEngineeringSymbolConnectorId<T>(this IRuleBuilder<T, string> ruleBuilder)
	{
		return ruleBuilder
			.NotNull()
			.NotEmpty()
			.MinimumLength(10)
			.MaximumLength(32)
			.Must(EngineeringSymbolValidation.ContainsOnlyLatinLetters)
			.WithMessage("Illegal characters detected. Only latin letters allowed.");
	}

	public static IRuleBuilderOptions<T, Point> MustBeValidEngineeringSymbolConnectorPoint<T>(this IRuleBuilder<T, Point> ruleBuilder)
	{
		return ruleBuilder
			.NotNull();
	}
	
	public static IRuleBuilderOptions<T, Point> MustBeValidEngineeringSymbolCenterOfRotation<T>(this IRuleBuilder<T, Point> ruleBuilder)
	{
		return ruleBuilder
			.NotNull();
	}

	public static IRuleBuilderOptions<T, int> MustBeValidEngineeringSymbolConnectorDirection<T>(this IRuleBuilder<T, int> ruleBuilder)
	{
		return ruleBuilder
			.NotNull()
			.GreaterThanOrEqualTo(0)
			.LessThanOrEqualTo(360);
	}
}