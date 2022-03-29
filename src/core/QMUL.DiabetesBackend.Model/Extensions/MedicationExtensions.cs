namespace QMUL.DiabetesBackend.Model.Extensions
{
    using Constants;
    using Hl7.Fhir.Model;

    /// <summary>
    /// Extension methods to <see cref="Medication"/>.
    /// </summary>
    public static class MedicationExtensions
    {
        /// <summary>
        /// Checks if the <see cref="Medication"/> has the insulin flag extension.
        /// </summary>
        /// <param name="medication">The <see cref="Medication"/>.</param>
        /// <returns>True if the medication has the insulin flag extension and its value is true. False otherwise</returns>
        public static bool HasInsulinFlag(this Medication medication)
        {
            return medication.GetBoolExtension(Extensions.InsulinFlag)?? false;
        }
    }
}