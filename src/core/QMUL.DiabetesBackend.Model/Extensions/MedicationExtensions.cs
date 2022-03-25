namespace QMUL.DiabetesBackend.Model.Extensions
{
    using Hl7.Fhir.Model;

    public static class MedicationExtensions
    {
        public static bool HasInsulinFlag(this MedicationRequest medicationRequest)
        {
            return medicationRequest.GetBoolExtension("http://diabetes-assistant.com/fhir/StructureDefinition/InsulinFlag")?? false;
        }

        // TODO flag the medication request when the medication is insulin
        public static void FlagInsulin(this MedicationRequest medicationRequest)
        {
            medicationRequest.SetBoolExtension("http://diabetes-assistant.com/fhir/StructureDefinition/InsulinFlag", true);
        }
    }
}