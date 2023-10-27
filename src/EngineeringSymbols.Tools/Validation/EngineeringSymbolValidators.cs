using EngineeringSymbols.Tools.Entities;
using FluentValidation;

namespace EngineeringSymbols.Tools.Validation;

public class EngineeringSymbolCreateDtoValidator : AbstractValidator<EngineeringSymbolPutDto>
{
    public EngineeringSymbolCreateDtoValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(symbol => symbol.Identifier)
            .MustBeValidEngineeringSymbolIdentifier();
        
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

        
        RuleForEach(symbol => symbol.Creators)
            .MustBeValidEngineeringSymbolUser()
            .When(symbol => symbol.Creators != null);
        
        RuleForEach(symbol => symbol.Contributors)
            .MustBeValidEngineeringSymbolUser()
            .When(symbol => symbol.Contributors != null);
        
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
                    if (string.IsNullOrEmpty(s.Value) || string.IsNullOrWhiteSpace(s.Value))
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
        RuleFor(key => key)
            .MustBeValidEngineeringSymbolIdentifier();
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