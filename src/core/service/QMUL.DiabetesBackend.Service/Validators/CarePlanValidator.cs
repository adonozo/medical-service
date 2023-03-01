namespace QMUL.DiabetesBackend.Service.Validators;

using FluentValidation;
using Hl7.Fhir.Model;

public class CarePlanValidator : ResourceValidatorBase<CarePlan>
{
    public CarePlanValidator()
    {
        RuleFor(plan => plan.Subject)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .SetValidator(new PatientSubjectValidator());
    }
}