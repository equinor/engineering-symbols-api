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

        RuleFor(symbol => symbol.Key).MustBeValidEngineeringSymbolKey();

        RuleFor(symbol => symbol.Status).MustBeValidEngineeringSymbolStatus();

        RuleFor(symbol => symbol.Owner).MustBeValidEngineeringSymbolOwner();

        RuleFor(symbol => symbol.Description).MustBeValidEngineeringSymbolDescription();

        RuleFor(symbol => symbol.DateTimeCreated).MustBeValidEngineeringSymbolDateCreated();

        RuleFor(symbol => symbol.DateTimeUpdated).MustBeValidEngineeringSymbolDateUpdated();

        RuleFor(symbol => symbol.DateTimePublished).MustBeValidEngineeringSymbolDatePublished();

        RuleFor(symbol => symbol.Geometry).MustBeValidEngineeringSymbolGeometry();

        RuleFor(symbol => symbol.Width).MustBeValidEngineeringSymbolWidth();

        RuleFor(symbol => symbol.Height).MustBeValidEngineeringSymbolHeight();

        RuleFor(symbol => symbol.Connectors)
            .MustBeValidEngineeringSymbolConnectorList()
            .ForEach(connectorRule =>
            {
                connectorRule.SetValidator(new EngineeringSymbolConnectorDtoValidator());
            });
    }
}

public class EngineeringSymbolCreateDtoValidator : AbstractValidator<EngineeringSymbolCreateDto>
{
    public EngineeringSymbolCreateDtoValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(symbol => symbol.Identifier).MustBeValidEngineeringSymbolIdentifier();
        
        RuleFor(symbol => symbol.IsRevisionOf)
            .MustBeValidSymbolIri()
            .When(symbol => !string.IsNullOrEmpty(symbol.IsRevisionOf));
        
        RuleFor(symbol => symbol.Label).MustBeValidEngineeringSymbolLabel();
        
        RuleFor(symbol => symbol.Description).MustBeValidEngineeringSymbolDescription();
        
        RuleFor(symbol => symbol.Sources).MustBeValidEngineeringSymbolSources();
        
        RuleFor(symbol => symbol.Subjects).MustBeValidEngineeringSymbolSubjects();
        
        RuleFor(symbol => symbol.Creators).MustBeValidEngineeringSymbolCreators();
        
        RuleFor(symbol => symbol.Contributors).MustBeValidEngineeringSymbolContributors();
        
        RuleFor(symbol => symbol.Shape).MustBeValidEngineeringSymbolShape();
        
        RuleFor(symbol => symbol.Width).MustBeValidEngineeringSymbolWidth();
        RuleFor(symbol => symbol.Height).MustBeValidEngineeringSymbolHeight();

        
        RuleFor(symbol => symbol.DrawColor).MustBeValidEngineeringSymbolDrawColor();
        
        RuleFor(symbol => symbol.FillColor).MustBeValidEngineeringSymbolFillColor();
        
        
        RuleFor(symbol => symbol.CenterOfRotation).MustBeValidEngineeringSymbolCenterOfRotation();
        
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