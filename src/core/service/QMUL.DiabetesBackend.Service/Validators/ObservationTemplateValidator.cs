namespace QMUL.DiabetesBackend.Service.Validators;

using FluentValidation;
using Model;

public class ObservationTemplateValidator : ValidatorBase<ObservationTemplate>
{
    public ObservationTemplateValidator()
    {
        RuleFor(observation => observation.Code)
            .NotNull();

        RuleFor(observation => observation.Code.Coding)
            .NotEmpty()
            .When(observation => observation.Code != null);

        RuleFor(observation => observation.ValueTemplate)
            .Must(quantity => quantity?.Code is not null && quantity.System is not null && quantity.Unit is not null)
            .When(observation => observation.ValueTemplate is not null);

        RuleFor(observation => observation.ReferenceRange)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .NotNull()
            .ForEach(referenceRules => referenceRules
                .Must(referenceRange => referenceRange.Low != null
                                        || referenceRange.High != null)
                .WithMessage("The reference value must have a low or a high value"));
    }
}