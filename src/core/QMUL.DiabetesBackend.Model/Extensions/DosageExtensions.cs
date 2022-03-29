namespace QMUL.DiabetesBackend.Model.Extensions
{
    using System;
    using Constants;
    using Hl7.Fhir.Model;

    public static class DosageExtensions
    {
        /// <summary>
        /// Gets the patient's <see cref="MedicationRequest"/> start date from the contained <see cref="Dosage"/>. This
        /// date is stored as an extension value within the dosage.
        /// </summary>
        /// <param name="dosage">The <see cref="Dosage"/> contained in the <see cref="MedicationRequest"/></param>
        /// <returns>The <see cref="DateTimeOffset"/> start date, or null if the value was not found.</returns>
        public static DateTimeOffset? GetStartDate(this Dosage dosage)
        {
            var extension = dosage.GetExtension(Extensions.DosageStartDate);
            var startDate = extension?.Value as FhirDateTime;
            if (startDate != null && startDate.TryToDateTimeOffset(out var result))
            {
                return result;
            }

            return null;
        }

        /// <summary>
        /// Holds the exact date for a resource to start. Should be used when the resource has a frequency rather than a
        /// period. For example, a medication that must be taken for 14 days.
        /// </summary>
        /// <param name="dosage">The medication service's dosage.</param>
        /// <param name="date">The date when this dosage has/will start</param>
        public static void SetStartDate(this Dosage dosage, DateTimeOffset date)
        {
            var fhirDate = new FhirDateTime(date);
            dosage.SetExtension(Extensions.DosageStartDate, fhirDate);
        }
    }
}