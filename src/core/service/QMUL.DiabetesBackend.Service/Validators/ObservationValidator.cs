namespace QMUL.DiabetesBackend.Service.Validators;

using FluentValidation;
using Hl7.Fhir.Model;

public class ObservationValidator : ResourceValidatorBase<Observation>
{
    public ObservationValidator()
    {
        RuleFor(observation => observation.Status)
            .NotNull();

        RuleFor(plan => plan.Subject)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .SetValidator(new PatientSubjectValidator());

        RuleFor(observation => observation.Code)
            .NotNull();

        RuleFor(observation => observation.Code.Coding)
            .NotEmpty()
            .When(observation => observation.Code != null);

        RuleFor(observation => observation.Value)
            .NotNull();

        RuleFor(observation => observation.Effective)
            .NotNull();

        RuleFor(observation => observation.Issued)
            .NotNull();

        RuleFor(observation => observation.ReferenceRange)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .ForEach(referenceRules => referenceRules
                .Must(referenceRange => referenceRange.Low != null || referenceRange.High != null
                                                                   || referenceRange.Text != null)
                .WithMessage("The reference value must have a low, a high, or a text value"));
    }
}