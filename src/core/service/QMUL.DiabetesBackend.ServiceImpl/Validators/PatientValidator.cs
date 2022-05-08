namespace QMUL.DiabetesBackend.ServiceImpl.Validators
{
    using FluentValidation;
    using Hl7.Fhir.Model;

    public class PatientValidator : ResourceValidatorBase<Patient>
    {
        public PatientValidator()
        {
            RuleFor(patient => patient.Name)
                .NotEmpty();

            RuleFor(patient => patient.Gender)
                .NotNull();

            RuleFor(patient => patient.BirthDate)
                .NotNull();
        }
    }
}