namespace QMUL.DiabetesBackend.ServiceImpl.Validators
{
    using System;
    using FluentValidation;
    using Hl7.Fhir.Model;
    using ServiceInterfaces;

    public class MedicationRequestValidator : ResourceValidatorBase<MedicationRequest>
    {
        public MedicationRequestValidator(IMedicationService medicationService)
        {
            RuleFor(request => request.Subject)
                .NotNull();

            RuleFor(request => request.Status)
                .NotNull();
            
            RuleFor(request => request.Medication)
                .Cascade(CascadeMode.Stop)
                .NotNull()
                .Must(medication => medication is ResourceReference { Reference: { } })
                .WithMessage("Medication field is not a Medication Reference");

            RuleFor(request => request.Contained)
                .NotEmpty()
                .ForEach(containedRules => containedRules
                    .Must(resource => resource is Medication)
                    .WithMessage("Contained elements must be of type Medication"))
                .Must((request, _) =>
                {
                    var reference = request.Medication as ResourceReference;
                    var medicationFound = request.FindContainedResource(reference?.Reference);
                    return medicationFound != null;
                })
                .WithMessage("The medication reference refers to a contained medication, but no match was found.")
                .When(request =>
                    request.Medication is ResourceReference { Reference: { } } reference && reference.Reference.StartsWith("#"));

            RuleFor(request => request.Medication)
                .MustAsync(async (requestMedication, _) =>
                {
                    var reference = requestMedication as ResourceReference;
                    try
                    {
                        await medicationService.GetMedication(reference?.Reference ?? string.Empty);
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                    
                    return true;
                })
                .WithMessage("The medication reference refers to a medication in the system, but it was not found.")
                .When(request =>
                    request.Medication is ResourceReference { Reference: { } } reference && !reference.Reference.StartsWith("#"));

            RuleFor(request => request.DosageInstruction)
                .NotEmpty()
                .ForEach(dosageRules => dosageRules
                    .SetValidator(new DosageValidator()));
        }
        
        private class DosageValidator : AbstractValidator<Dosage>
        {
            public DosageValidator()
            {
                RuleFor(dosage => dosage.DoseAndRate)
                    .NotEmpty();

                RuleFor(dosage => dosage.Timing)
                    .NotNull()
                    .SetValidator(new TimingValidator());
            }
        }
    }
}