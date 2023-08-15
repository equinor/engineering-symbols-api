using EngineeringSymbols.Tools.Entities;
using FluentValidation;

namespace EngineeringSymbols.Tools.Validation;

public class EngineeringSymbolDtoValidator : AbstractValidator<EngineeringSymbolDto>
{
    public EngineeringSymbolDtoValidator()
    {
        RuleFor(symbol => symbol.Id)
            .NotNull()
            .Must(s => Guid.TryParse(s, out _));

        RuleFor(symbol => symbol.Key)
            .NotNull()
            .MinimumLength(4)
            .MaximumLength(250)
            .Matches("^[a-zA-Z0-9]+$");
        
        RuleFor(symbol => symbol.Status)
            .NotNull()
            .Must(status => Enum.TryParse<EngineeringSymbolStatus>(status, out _));

        RuleFor(symbol => symbol.Owner)
            .NotNull()
            .EmailAddress();

        RuleFor(symbol => symbol.Description)
            .NotNull()
            .MinimumLength(4)
            .MaximumLength(250)
            .Must(s => !EngineeringSymbolValidation.ContainsIllegalChars(s));

        RuleFor(symbol => symbol.Filename)
            .NotNull();

        RuleFor(symbol => symbol.DateTimeCreated)
            .NotNull()
            .Must(d => d != DateTimeOffset.MinValue);


        RuleFor(symbol => symbol.DateTimeUpdated)
            .NotNull()
            .Must((dto, dateUpdated) => dateUpdated > dto.DateTimeCreated)
            .When(dto => dto.DateTimeUpdated != DateTimeOffset.MinValue);
        
        RuleFor(symbol => symbol.DateTimePublished)
            .NotNull()
            .Must((dto, datePublished) => datePublished > dto.DateTimeCreated)
            .When(dto => dto.DateTimePublished != DateTimeOffset.MinValue);

        RuleFor(symbol => symbol.Geometry)
            .NotNull()
            .NotEmpty();

        RuleFor(symbol => symbol.Width)
            .NotNull()
            .Must(w => w % 24 == 0)
            .WithMessage($"SVG 'width' is not a multiple of 24");
        
        RuleFor(symbol => symbol.Height)
            .NotNull()
            .Must(h => h % 24 == 0)
            .WithMessage($"SVG 'height' is not a multiple of 24");

    }
}