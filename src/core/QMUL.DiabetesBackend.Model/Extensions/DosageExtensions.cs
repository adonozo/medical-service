namespace QMUL.DiabetesBackend.Model.Extensions
{
    using System;
    using Hl7.Fhir.Model;

    public static class DosageExtensions
    {
        public static DateTimeOffset? GetStartDate(this Dosage dosage)
        {
            var extension =
                dosage.GetExtension("http://diabetes-assistant.com/fhir/StructureDefinition/DosageStartDate");
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
            dosage.SetExtension("http://diabetes-assistant.com/fhir/StructureDefinition/DosageStartDate", fhirDate);
        }
    }
}
