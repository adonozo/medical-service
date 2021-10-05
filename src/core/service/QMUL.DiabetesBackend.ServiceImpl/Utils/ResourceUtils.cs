namespace QMUL.DiabetesBackend.ServiceImpl.Utils
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Hl7.Fhir.Model;
    using Model.Enums;

    /// <summary>
    /// Resource util methods
    /// </summary>
    public static class ResourceUtils
    {
        /// <summary>
        /// Creates an empty Bundle of type <see cref="Bundle.BundleType.Searchset"/> and time set to UtcNow
        /// </summary>
        /// <returns>A Bundle object</returns>
        public static Bundle GenerateEmptyBundle()
        {
            return new()
            {
                Type = Bundle.BundleType.Searchset,
                Timestamp = DateTimeOffset.UtcNow
            };
        }

        /// <summary>
        /// Checks if the medication request contains an Insulin medication.
        /// </summary>
        /// <param name="request">The Medication Request</param>
        /// <returns>True if the medication request contains insulin.</returns>
        public static bool IsInsulinResource(MedicationRequest request)
        {
            try
            {
                var extensions = request.Extension;
                return extensions != null && extensions.Any(extension => extension.Url.ToLower().Contains("insulin"));
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static async Task<T> ValidateObject<T>(Func<Task<T>> getObjectFunction, string message, Exception exception)
        {
            var result = await getObjectFunction.Invoke();
            if (result == null)
            {
                throw exception;
            }

            return result;
        }

        public static EventType MapRequestToEventType(AlexaRequestType requestType)
        {
            return requestType switch
            {
                AlexaRequestType.Medication => EventType.MedicationDosage,
                AlexaRequestType.Insulin => EventType.InsulinDosage,
                AlexaRequestType.Glucose => EventType.Measurement,
                _ => throw new ArgumentOutOfRangeException(nameof(requestType), requestType, "Invalid request type")
            };
        }
    }
}