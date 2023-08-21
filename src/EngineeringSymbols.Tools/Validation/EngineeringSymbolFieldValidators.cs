using FluentValidation;
using J2N;

namespace EngineeringSymbols.Tools.Validation;

public static class EngineeringSymbolFieldValidators
{
    public static readonly char[] TextWhiteList = {'-', '_', '.', ',', '!', '?'};

    public const int BaseSvgSize = 24;
    
    public static IRuleBuilderOptions<T, string> MustBeValidEngineeringSymbolId<T>(this IRuleBuilder<T, string> ruleBuilder) {
        return ruleBuilder
            .Must(s => Guid.TryParse(s, out _))
            .WithMessage("Is not a valid Engineering Symbol Id");
    }
    
    public static IRuleBuilderOptions<T, string> MustBeValidEngineeringSymbolKey<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotNull()
            .MinimumLength(4)
            .MaximumLength(64)
            .Must(s => !EngineeringSymbolValidation.ContainsIllegalChars(s, new[] {'-', '_'}));
        //.WithMessage("Is not a valid Engineering Symbol Key");
    }
    
    public static IRuleBuilderOptions<T, string> MustBeValidEngineeringSymbolStatus<T>(this IRuleBuilder<T, string> ruleBuilder) {
        return ruleBuilder
            .Must(status => Enum.TryParse<EngineeringSymbolStatus>(status, out _))
            .WithMessage("Invalid Engineering Symbol Status");
    }
    
    public static IRuleBuilderOptions<T, string> MustBeValidEngineeringSymbolOwner<T>(this IRuleBuilder<T, string> ruleBuilder) {
        return ruleBuilder
            .NotNull()
            .EmailAddress()
            .WithMessage("Invalid value. Must be a valid email address.");
    }
    
    public static IRuleBuilderOptions<T, string> MustBeValidEngineeringSymbolDescription<T>(this IRuleBuilder<T, string> ruleBuilder) {
        return ruleBuilder
            .NotNull()
            //.MinimumLength(4)
            .MaximumLength(250)
            .Must(s => !EngineeringSymbolValidation.ContainsIllegalChars(s, TextWhiteList))
            .WithMessage("Invalid value.");
    }

    public static IRuleBuilderOptions<T, DateTimeOffset> MustBeValidEngineeringSymbolDateCreated<T>(this IRuleBuilder<T, DateTimeOffset> ruleBuilder) {
        return ruleBuilder
            .NotNull()
            .Must(d => d != DateTimeOffset.MinValue);
    }
    
    public static IRuleBuilderOptions<T, DateTimeOffset> MustBeValidEngineeringSymbolDateUpdated<T>(this IRuleBuilder<T, DateTimeOffset> ruleBuilder) {
        return ruleBuilder
            .NotNull();
    }
    
    public static IRuleBuilderOptions<T, DateTimeOffset> MustBeValidEngineeringSymbolDatePublished<T>(this IRuleBuilder<T, DateTimeOffset> ruleBuilder) {
        return ruleBuilder
            .NotNull();
    }
    
    public static IRuleBuilderOptions<T, string> MustBeValidEngineeringSymbolGeometry<T>(this IRuleBuilder<T, string> ruleBuilder) {
        return ruleBuilder
            .NotNull()
            .NotEmpty();
    }
    
    public static IRuleBuilderOptions<T, double> MustBeValidEngineeringSymbolWidth<T>(this IRuleBuilder<T, double> ruleBuilder) {
        return ruleBuilder
            .NotNull()
            .GreaterThanOrEqualTo(BaseSvgSize)
            .WithMessage($"'Width' must not be empty, or have a value less than {BaseSvgSize}.")
            .Must(w => w % BaseSvgSize == 0)
            .WithMessage($"'Width' is not a multiple of {BaseSvgSize}");
    }
    
    public static IRuleBuilderOptions<T, double> MustBeValidEngineeringSymbolHeight<T>(this IRuleBuilder<T, double> ruleBuilder) {
        return ruleBuilder
            .NotNull()
            .GreaterThanOrEqualTo(BaseSvgSize)
            .WithMessage($"'Height' must not be empty, or have a value less than {BaseSvgSize}.")
            .Must(h => h % BaseSvgSize == 0)
            .WithMessage($"'Height' is not a multiple of {BaseSvgSize}");
    }
    
    public static IRuleBuilderOptions<T, string> MustBeValidEngineeringSymbolConnectorId<T>(this IRuleBuilder<T, string> ruleBuilder) {
            return ruleBuilder
                .NotNull()
                .NotEmpty();
    }
    
    public static IRuleBuilderOptions<T, Point> MustBeValidEngineeringSymbolConnectorPoint<T>(this IRuleBuilder<T, Point> ruleBuilder) {
        return ruleBuilder
            .NotNull();
    }
    
    public static IRuleBuilderOptions<T, int> MustBeValidEngineeringSymbolConnectorDirection<T>(this IRuleBuilder<T, int> ruleBuilder) {
        return ruleBuilder
            .NotNull()
            .GreaterThanOrEqualTo(0)
            .LessThanOrEqualTo(360);
    }
}