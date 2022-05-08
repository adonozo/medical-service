namespace QMUL.DiabetesBackend.ServiceImpl.Validators
{
    using FluentValidation;
    using Hl7.Fhir.Model;

    internal class TimingValidator : AbstractValidator<Timing>
    {
        public TimingValidator()
        {
            RuleFor(timing => timing.Repeat)
                .NotNull();

            RuleFor(timing => timing.Repeat.Bounds)
                .NotNull()
                .Must(bounds => bounds is Period or Duration)
                .WithMessage("Repeat.Bounds must be of type Period or Duration")
                .When(timing => timing.Repeat != null);
            
            RuleFor(timing => timing.Repeat.Bounds)
                .NotNull()
                .Must(bounds => bounds is Period { Start: { }, End: { } })
                .WithMessage("The start and end date period bounds must not be empty")
                .When(timing => timing.Repeat is { Bounds: Period });
            
            RuleFor(timing => timing.Repeat.Bounds)
                .NotNull()
                .Must(bounds => bounds is Duration { Value: { } })
                .WithMessage("The bounds duration must not be empty")
                .When(timing => timing.Repeat is { Bounds: Duration });

            RuleFor(timing => timing.Repeat.Period)
                .NotNull()
                .When(timing => timing.Repeat != null);

            RuleFor(timing => timing.Repeat.PeriodUnit)
                .NotNull()
                .Equal(Timing.UnitsOfTime.D)
                .When(timing => timing.Repeat != null);
        }
    }
}