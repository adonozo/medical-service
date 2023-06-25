namespace QMUL.DiabetesBackend.Service.Validators;

using System.Linq;
using FluentValidation;
using Hl7.Fhir.Model;

public class ServiceRequestValidator : ResourceValidatorBase<ServiceRequest>
{
    public ServiceRequestValidator(bool validateContained = true)
    {
        RuleFor(request => request.Status)
            .NotNull();

        RuleFor(request => request.Intent)
            .NotNull();

        RuleFor(plan => plan.Subject)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .SetValidator(new PatientSubjectValidator());

        RuleFor(request => request.Occurrence)
            .NotNull()
            .Must(occurrence => occurrence is Timing)
            .WithMessage("Occurrence must be of type Timing");

        Transform(request => request.Occurrence, occurrence => occurrence as Timing ?? new Timing())
            .NotEmpty()
            .SetValidator(new TimingValidator());

        if (!validateContained)
        {
            return;
        }

        RuleFor(request => request.Contained)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .Must(contained => contained.All(r => r is ServiceRequest))
            .WithMessage("Contained elements are not of type ServiceRequest");

        RuleForEach(request => request.Contained.Select(r => (r as ServiceRequest) ?? new ServiceRequest()).ToArray())
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .OverridePropertyName("ServiceRequest")
            .SetValidator(new ServiceRequestValidator(false));
    }
}