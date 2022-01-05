namespace QMUL.DiabetesBackend.ServiceInterfaces
{
    using System;
    using System.Threading.Tasks;
    using Hl7.Fhir.Model;
    using Model.Enums;
    using Task = System.Threading.Tasks.Task;

    /// <summary>
    /// The Alexa Service Interface. Manages Alexa related requests.
    /// </summary>
    public interface IAlexaService
    {
        /// <summary>
        /// Gets the medication requests for a given patient based on a date, timing, and the user's timezone. It does not
        /// include the insulin requests. The results are limited to a single day timespan due to CustomEventTiming.
        /// </summary>
        /// <param name="patientEmailOrId">The patient's unique email or ID</param>
        /// <param name="dateTime">The date and time to get the results from</param>
        /// <param name="timing">A <see cref="CustomEventTiming"/> to limit the results to a timing in the day</param>
        /// <param name="timezone">The user's timezone. Defaults to UTC</param>
        /// <returns>A <see cref="Bundle"/> with the results</returns>
        public Task<Bundle> ProcessMedicationRequest(string patientEmailOrId, DateTime dateTime, CustomEventTiming timing,
            string timezone = "UTC");
        
        /// <summary>
        /// Gets the insulin medication requests for a given patient based on a date, timing, and the user's timezone.
        /// The results are limited to a single day timespan due to CustomEventTiming.
        /// </summary>
        /// <param name="patientEmailOrId">The patient's unique email or ID</param>
        /// <param name="dateTime">The date and time to get the results from</param>
        /// <param name="timing">A <see cref="CustomEventTiming"/> to limit the results to a timing in the day</param>
        /// <param name="timezone">The user's timezone. Defaults to UTC</param>
        /// <returns>A <see cref="Bundle"/> with the results</returns>
        public Task<Bundle> ProcessInsulinMedicationRequest(string patientEmailOrId, DateTime dateTime, CustomEventTiming timing,
            string timezone = "UTC");
        
        /// <summary>
        /// Gets the glucose service requests for a given patient based on a date, timing, and the user's timezone.
        /// The results are limited to a single day timespan due to CustomEventTiming.
        /// </summary>
        /// <param name="patientEmailOrId">The patient's unique email or ID</param>
        /// <param name="dateTime">The date and time to get the results from</param>
        /// <param name="timing">A <see cref="CustomEventTiming"/> to limit the results to a timing in the day</param>
        /// <param name="timezone">The user's timezone. Defaults to UTC</param>
        /// <returns>A <see cref="Bundle"/> with the results</returns>
        public Task<Bundle> ProcessGlucoseServiceRequest(string patientEmailOrId, DateTime dateTime, CustomEventTiming timing,
            string timezone = "UTC");
        
        /// <summary>
        /// Gets the complete care plan for a given patient based on a date, timing, and the user's timezone. This means
        /// medication (insulin or not) and service requests. The results are limited to a single day timespan due to
        /// CustomEventTiming.
        /// </summary>
        /// <param name="patientEmailOrId">The patient's unique email or ID</param>
        /// <param name="dateTime">The date and time to get the results from</param>
        /// <param name="timing">A <see cref="CustomEventTiming"/> to limit the results to a timing in the day</param>
        /// <param name="timezone">The user's timezone. Defaults to UTC</param>
        /// <returns>A <see cref="Bundle"/> with the results</returns>
        public Task<Bundle> ProcessCarePlanRequest(string patientEmailOrId, DateTime dateTime, CustomEventTiming timing,
            string timezone = "UTC");

        /// <summary>
        /// Gets the next requests for a patient to follow given a request type.
        /// TODO this method should accept a result limit or have a default value. 
        /// </summary>
        /// <param name="patientEmailOrId">The patient's ID or email who owns the requests.</param>
        /// <param name="type">The <see cref="AlexaRequestType"/></param>
        /// <returns>A <see cref="Bundle"/> object with the list of requests.</returns>
        public Task<Bundle> GetNextRequests(string patientEmailOrId, AlexaRequestType type);

        /// <summary>
        /// Gets the next requests for a patient to follow without filtering the request type.
        /// TODO this method should accept a result limit or have a default value.
        /// </summary>
        /// <param name="patientEmailOrId">The patient's ID or email who owns the requests.</param>
        /// <returns>A <see cref="Bundle"/> object with the list of requests.</returns>
        public Task<Bundle> GetNextRequests(string patientEmailOrId);

        /// <summary>
        /// Updates / Adds a specific time for a event timing to the patient's list. e.g., a specific time for breakfast.  
        /// </summary>
        /// <param name="patientIdOrEmail">The patient's ID or email.</param>
        /// <param name="eventTiming">The event timing to set.</param>
        /// <param name="dateTime">The time for the event. The date is ignored.</param>
        /// <returns>A boolean value to indicate is the update was successful.</returns>
        public Task<bool> UpsertTimingEvent(string patientIdOrEmail, CustomEventTiming eventTiming, DateTime dateTime);

        /// <summary>
        /// Updates / Adds a start date for a dosage instruction. Useful when the medication doesn't have an exact start
        /// date and it has a duration rather than a period, e.g., for 7 days, for a month, etc.
        /// The associated health events are updated as well by deleting old events and creating new ones. 
        /// </summary>
        /// <param name="patientIdOrEmail">The patient's ID or email.</param>
        /// <param name="dosageId">The dosage ID to update.</param>
        /// <param name="startDate">The start date.</param>
        /// <returns>A boolean value to indicate is the update was successful.</returns>
        Task<bool> UpsertDosageStartDate(string patientIdOrEmail, string dosageId, DateTime startDate);
    }
}