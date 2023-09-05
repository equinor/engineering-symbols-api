using EngineeringSymbols.Tools.Entities;
using EngineeringSymbols.Tools.Validation;
using FluentValidation;

namespace EngineeringSymbols.Tools.Validation;

public class EngineeringSymbolDtoValidator : AbstractValidator<EngineeringSymbolDto>
{
    public EngineeringSymbolDtoValidator()
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

        RuleFor(symbol => symbol.Key).MustBeValidEngineeringSymbolKey();

        RuleFor(symbol => symbol.Owner).MustBeValidEngineeringSymbolOwner();

        RuleFor(symbol => symbol.Description).MustBeValidEngineeringSymbolDescription();

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

public class EngineeringSymbolConnectorDtoValidator : AbstractValidator<EngineeringSymbolConnectorDto>
{
    public EngineeringSymbolConnectorDtoValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(connector => connector.Id).MustBeValidEngineeringSymbolConnectorId();
        RuleFor(connector => connector.RelativePosition).MustBeValidEngineeringSymbolConnectorPoint();
        RuleFor(connector => connector.Direction).MustBeValidEngineeringSymbolConnectorDirection();
    }
}

public class EngineeringSymbolKeyValidator : AbstractValidator<string>
{
    public EngineeringSymbolKeyValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        RuleFor(key => key).MustBeValidEngineeringSymbolKey();
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