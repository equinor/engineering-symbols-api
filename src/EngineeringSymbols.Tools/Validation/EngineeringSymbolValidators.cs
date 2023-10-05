using EngineeringSymbols.Tools.Entities;
using EngineeringSymbols.Tools.Validation;
using FluentValidation;

namespace EngineeringSymbols.Tools.Validation;

public class EngineeringSymbolValidator : AbstractValidator<EngineeringSymbol>
{
    public EngineeringSymbolValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(symbol => symbol.Id).MustBeValidEngineeringSymbolId();

        RuleFor(symbol => symbol.Identifier)
            .MustBeValidEngineeringSymbolIdentifier();

        //RuleFor(symbol => symbol.Status).MustBeValidEngineeringSymbolStatus();
        
        RuleFor(symbol => symbol.Version).MustBeValidEngineeringSymbolVersion();
        
        RuleFor(symbol => symbol.PreviousVersion)
            .MustBeValidSymbolIri()
            .When(symbol => symbol.PreviousVersion != null);

        RuleFor(symbol => symbol.Label).MustBeValidEngineeringSymbolLabel();
        
        RuleFor(symbol => symbol.Description)
            .MustBeValidEngineeringSymbolDescription();

        RuleForEach(symbol => symbol.Sources)
            .MustBeValidEngineeringSymbolSource()
            .When(symbol => symbol.Sources != null);
        
        RuleForEach(symbol => symbol.Subjects)
            .MustBeValidEngineeringSymbolSubject()
            .When(symbol => symbol.Subjects != null);
        
        RuleFor(symbol => symbol.DateTimeCreated).MustBeValidEngineeringSymbolDateCreated();

        RuleFor(symbol => symbol.DateTimeModified).MustBeValidEngineeringSymbolDateModified();

        RuleFor(symbol => symbol.DateTimeIssued).MustBeValidEngineeringSymbolDateIssued();
        
        RuleFor(symbol => symbol.Creators)
            .NotNull()
            .NotEmpty()
            .ForEach(s => s.MustBeValidEngineeringSymbolUser());

        RuleFor(symbol => symbol.Contributors)
            .NotNull()
            .ForEach(s => s.MustBeValidEngineeringSymbolUser());

        
        RuleFor(symbol => symbol.Shape)
            .NotNull()
            .SetValidator(new EngineeringSymbolShapeValidator());

        RuleFor(symbol => symbol.Width).MustBeValidEngineeringSymbolWidth();

        RuleFor(symbol => symbol.Height).MustBeValidEngineeringSymbolHeight();

        RuleFor(symbol => symbol.DrawColor)
            .MustBeValidEngineeringSymbolColor()
            .When(s => s.DrawColor != null);

        RuleFor(symbol => symbol.FillColor)
            .MustBeValidEngineeringSymbolColor()
            .When(s => s.FillColor != null);
        
        RuleFor(symbol => symbol.CenterOfRotation)
            .MustBeValidEngineeringSymbolCenterOfRotation();
        
        RuleFor(symbol => symbol.ConnectionPoints)
            .MustBeValidEngineeringSymbolConnectionPointsList()
            .ForEach(connectorRule =>
            {
                connectorRule.SetValidator(new EngineeringSymbolConnectionPointValidator());
            });
    }
}

public class EngineeringSymbolCreateDtoValidator : AbstractValidator<EngineeringSymbolCreateDto>
{
    public EngineeringSymbolCreateDtoValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(symbol => symbol.Identifier)
            .MustBeValidEngineeringSymbolIdentifier();
        
        RuleFor(symbol => symbol.IsRevisionOf)
            .MustBeValidSymbolIri()
            .When(symbol => !string.IsNullOrEmpty(symbol.IsRevisionOf));
        
        RuleFor(symbol => symbol.Label)
            .MustBeValidEngineeringSymbolLabel();
        
        RuleFor(symbol => symbol.Description)
            .MustBeValidEngineeringSymbolDescription();

        RuleForEach(symbol => symbol.Sources)
            .MustBeValidEngineeringSymbolSource()
            .When(symbol => symbol.Sources != null);
        
        RuleForEach(symbol => symbol.Subjects)
            .MustBeValidEngineeringSymbolSubject()
            .When(symbol => symbol.Subjects != null);

        RuleFor(symbol => symbol.Creators)
            .NotNull()
            .NotEmpty()
            .ForEach(s => s.MustBeValidEngineeringSymbolUser());
        
        RuleFor(symbol => symbol.Contributors)
            .NotNull()
            .ForEach(s => s.MustBeValidEngineeringSymbolUser());


        RuleFor(symbol => symbol.Shape)
            .NotNull()
            .SetValidator(new EngineeringSymbolShapeValidator());
        
        RuleFor(symbol => symbol.Width)
            .MustBeValidEngineeringSymbolWidth();
        RuleFor(symbol => symbol.Height)
            .MustBeValidEngineeringSymbolHeight();
        
        RuleFor(symbol => symbol.DrawColor)
            .MustBeValidEngineeringSymbolColor()
            .When(s => s.DrawColor != null);

        RuleFor(symbol => symbol.FillColor)
            .MustBeValidEngineeringSymbolColor()
            .When(s => s.FillColor != null);
        
        RuleFor(symbol => symbol.CenterOfRotation)
            .MustBeValidEngineeringSymbolCenterOfRotation();
        
        RuleFor(symbol => symbol.ConnectionPoints)
            .MustBeValidEngineeringSymbolConnectionPointsList()
            .ForEach(connectorRule =>
            {
                connectorRule.SetValidator(new EngineeringSymbolConnectionPointValidator());
            });
    }
}

public class EngineeringSymbolConnectionPointValidator : AbstractValidator<ConnectionPoint>
{
    public EngineeringSymbolConnectionPointValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(connector => connector.Identifier).MustBeValidEngineeringSymbolConnectorId();
        RuleFor(connector => connector.Position).MustBeValidEngineeringSymbolConnectorPoint();
        RuleFor(connector => connector.Direction).MustBeValidEngineeringSymbolConnectorDirection();
    }
}

public class EngineeringSymbolShapeValidator : AbstractValidator<Shape>
{
    public EngineeringSymbolShapeValidator()
    {
        //ClassLevelCascadeMode = CascadeMode.Stop;
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(shape => shape.Serializations)
            .NotNull()
            .NotEmpty();
        
        RuleForEach(shape => shape.Serializations)
            .NotNull()
            .Must(s =>
            {
                if (s.Type == ShapeSerializationType.SvgPathData)
                {
                    if (string.IsNullOrEmpty(s.Serialization) || string.IsNullOrWhiteSpace(s.Serialization))
                    {
                        return false;
                    }
                }

                return true;
            })
            .WithMessage("Shape Serialization is invalid/incomplete")
            .When(s => s.Serializations != null);
        
        // TODO: Implement validation for depictions
        //RuleFor(connector => connector.Depictions)

    }
}

public class EngineeringSymbolIdentifierValidator : AbstractValidator<string>
{
    public EngineeringSymbolIdentifierValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        RuleFor(key => key).MustBeValidEngineeringSymbolIdentifier();
    }
}

public class EngineeringSymbolIdValidator : AbstractValidator<string>
{
    public EngineeringSymbolIdValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        RuleFor(key => key).MustBeValidEngineeringSymbolId();
    }
}