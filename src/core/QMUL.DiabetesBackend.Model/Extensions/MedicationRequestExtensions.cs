namespace QMUL.DiabetesBackend.Model.Extensions
{
    using Constants;
    using Hl7.Fhir.Model;

    /// <summary>
    /// Extension methods for <see cref="MedicationRequest"/>.
    /// </summary>
    public static class MedicationRequestExtensions
    {
        /// <summary>
        /// Checks if the <see cref="MedicationRequest"/> has the insulin flag extension. This means the medication request
        /// has an insulin type <see cref="Medication"/>.
        /// </summary>
        /// <param name="medicationRequest">The <see cref="MedicationRequest"/>.</param>
        /// <returns>True if the medication request has the insulin flag extension and its value is true. False otherwise</returns>
        public static bool HasInsulinFlag(this MedicationRequest medicationRequest)
        {
            return medicationRequest.GetBoolExtension(Extensions.InsulinFlag)?? false;
        }

        public static void SetInsulinFlag(this MedicationRequest medicationRequest)
        {
            medicationRequest.SetBoolExtension(Extensions.InsulinFlag, true);
        }
    }
}