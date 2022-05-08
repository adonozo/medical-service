namespace QMUL.DiabetesBackend.ServiceImpl.Validators
{
    using FluentValidation;
    using Hl7.Fhir.Model;

    public class ServiceRequestValidator : ResourceValidatorBase<ServiceRequest>
    {
        public ServiceRequestValidator()
        {
            RuleFor(request => request.Status)
                .NotNull();

            RuleFor(request => request.Intent)
                .NotNull();

            RuleFor(request => request.Subject)
                .NotNull();

            RuleFor(request => request.Occurrence)
                .NotNull()
                .Must(occurrence => occurrence is Timing)
                .WithMessage("Occurrence must be of type Timing");

            Transform(request => request.Occurrence, occurrence => occurrence as Timing ?? new Timing())
                .NotEmpty()
                .SetValidator(new TimingValidator());
        }
    }
}
