namespace QMUL.DiabetesBackend.ServiceImpl.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Hl7.Fhir.Model;
    using Model;
    using Model.Enums;
    using Task = System.Threading.Tasks.Task;

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
            return new Bundle
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

        /// <summary>
        /// Checks if the return value of an awaitable <see cref="Task{TResult}"/> is null. If it is not, it will return
        /// the object; otherwise, it will throw an exception set as an argument.
        /// </summary>
        /// <param name="getObjectFunction">A <see cref="Func{TResult}"/> that returns an awaitable <see cref="Task{TResult}"/></param>
        /// <param name="exception">The exception to throw if the result is null.</param>
        /// <typeparam name="T">The expected object's type</typeparam>
        /// <returns>The Func return value if it is not null.</returns>
        /// <exception cref="Exception">A custom exception if the Func result is null.</exception>
        public static async Task<T> ValidateNullObject<T>(Func<Task<T>> getObjectFunction, Exception exception)
        {
            var result = await getObjectFunction.Invoke();
            if (result == null)
            {
                throw exception;
            }

            return result;
        }

        /// <summary>
        /// Checks if the result of the awaitable method is true. If it is not, it throws the second argument exception.
        /// </summary>
        /// <param name="booleanFunction">The function that returns a boolean value. A successful operation should return true.</param>
        /// <param name="exception">The exception to throw if the result is false.</param>
        /// <returns>True if the result was successful.</returns>
        /// <exception cref="Exception">When the boolean result is false</exception>
        public static async Task ValidateBooleanResult(Func<Task<bool>> booleanFunction, Exception exception)
        {
            var result = await booleanFunction.Invoke();
            if (!result)
            {
                throw exception;
            }
        }

        /// <summary>
        /// Maps an AlexaRequestType to an EventType when there is an equivalence. Otherwise, throws an exception.
        /// </summary>
        /// <param name="requestType">The <see cref="AlexaRequestType"/> request.</param>
        /// <returns>An equivalent <see cref="EventType"/> for the Alexa request.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if there is no equivalence between the Alexa request
        /// and an Event type</exception>
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

        /// <summary>
        /// Creates a list of events based on medication timings.
        /// </summary>
        /// <param name="request">The medication request</param>
        /// <param name="patient">The medication request's subject</param>
        /// <returns>A List of events for the medication request</returns>
        public static IEnumerable<HealthEvent> GenerateEventsFrom(MedicationRequest request, Model.Patient patient)
        {
            var events = new List<HealthEvent>();
            var isInsulin = IsInsulinResource(request);

            foreach (var dosage in request.DosageInstruction)
            {
                var requestReference = new CustomResource
                {
                    EventType = isInsulin ? EventType.InsulinDosage : EventType.MedicationDosage,
                    ResourceId = request.Id,
                    Text = dosage.Text,
                    EventReferenceId = dosage.ElementId
                };
                var eventsGenerator = new EventsGenerator(patient, dosage.Timing, requestReference);
                events.AddRange(eventsGenerator.GetEvents());
            }

            return events;
        }

        /// <summary>
        /// Creates a list of <see cref="HealthEvent"/> from a <see cref="ServiceRequest"/>. The service request's
        /// occurrence must be an instance of <see cref="Hl7.Fhir.Model.Timing"/> to have consistency between
        /// service and medication requests, and to narrow down timing use cases.
        /// </summary>
        /// <param name="request">The medication request</param>
        /// <param name="patient">The medication request's subject</param>
        /// <returns>A List of events for the medication request</returns>
        public static IEnumerable<HealthEvent> GenerateEventsFrom(ServiceRequest request, Model.Patient patient)
        {
            var events = new List<HealthEvent>();
            var requestReference = new CustomResource
            {
                EventType = EventType.Measurement,
                ResourceId = request.Id,
                EventReferenceId = request.Id,
                Text = request.PatientInstruction
            };

            // Occurrence must be expressed as a timing instance
            if (request.Occurrence is not Timing timing)
            {
                throw new InvalidOperationException("Service Request Occurrence must be a Timing instance");
            }

            var eventsGenerator = new EventsGenerator(patient, timing, requestReference);
            events.AddRange(eventsGenerator.GetEvents());
            return events;
        }
    }
}