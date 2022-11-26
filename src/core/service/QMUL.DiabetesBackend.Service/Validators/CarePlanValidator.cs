namespace QMUL.DiabetesBackend.Service.Validators;

using FluentValidation;
using Hl7.Fhir.Model;
using Model.Constants;

public class CarePlanValidator : ResourceValidatorBase<CarePlan>
{
    public CarePlanValidator()
    {
        RuleFor(plan => plan.Subject)
            .NotNull();

        RuleFor(plan => plan.Subject)
            .Must(subject => subject.Reference.StartsWith(Constants.PatientPath))
            .WithMessage("Subject must reference a Patient resource")
            .When(subject => subject is not null);
    }
}