namespace QMUL.DiabetesBackend.Model.Extensions
{
    using Constants;
    using Hl7.Fhir.Model;

    public static class MedicationExtensions
    {
        public static bool HasInsulinFlag(this MedicationRequest medicationRequest)
        {
            return medicationRequest.GetBoolExtension(Extensions.InsulinFlag)?? false;
        }

        // TODO flag the medication request when the medication is insulin
        public static void FlagInsulin(this MedicationRequest medicationRequest)
        {
            medicationRequest.SetBoolExtension(Extensions.InsulinFlag, true);
        }
    }
}