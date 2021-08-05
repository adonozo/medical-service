using System;
using System.Linq;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using QMUL.DiabetesBackend.Model;
using QMUL.DiabetesBackend.Model.Enums;
using Task = System.Threading.Tasks.Task;

namespace QMUL.DiabetesBackend.ServiceImpl.Utils
{
    public static class ResourceUtils
    {
        public static DomainResource GetResourceFromEvent(this HealthEvent healthEvent)
        {
            return healthEvent.Resource.EventType switch
            {
                EventType.MedicationDosage => new MedicationRequest() ,
                EventType.InsulinDosage => new MedicationRequest(),
                EventType.Measurement => new ServiceRequest(),
                _ => throw new ArgumentOutOfRangeException(nameof(healthEvent))
            };
        }
        
        public static Bundle GenerateEmptyBundle()
        {
            return new()
            {
                Type = Bundle.BundleType.Searchset,
                Timestamp = DateTimeOffset.UtcNow
            };
        }

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
    }
}