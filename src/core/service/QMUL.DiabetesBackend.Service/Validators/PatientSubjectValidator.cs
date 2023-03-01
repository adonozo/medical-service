namespace QMUL.DiabetesBackend.Service.Validators;

using FluentValidation;
using Hl7.Fhir.Model;
using Model.Constants;

public class PatientSubjectValidator : AbstractValidator<ResourceReference>
{
    public PatientSubjectValidator()
    {
        RuleFor(subject => subject.Reference)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .Must(reference => reference.StartsWith(Constants.PatientPath))
            .WithMessage("Subject must reference a Patient resource");
    }
}