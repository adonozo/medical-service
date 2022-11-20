namespace QMUL.DiabetesBackend.Service.Validators
{
    using FluentValidation;
    using Hl7.Fhir.Model;

    public class MedicationValidator : ResourceValidatorBase<Medication>
    {
        public MedicationValidator()
        {
            RuleFor(medication => medication.Code)
                .NotNull();

            RuleFor(medication => medication.Code.Coding)
                .NotEmpty()
                .ForEach(codingRule => codingRule.NotNull())
                .When(medication => medication.Code != null);
        }
    }
}