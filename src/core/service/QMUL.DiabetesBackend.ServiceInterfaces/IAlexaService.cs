namespace QMUL.DiabetesBackend.ServiceInterfaces
{
    using System;
    using System.Threading.Tasks;
    using Exceptions;
    using Hl7.Fhir.Model;
    using Model.Enums;

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
        /// <exception cref="NotFoundException">If the patient was not found.</exception>
        public Task<Bundle> ProcessMedicationRequest(string patientEmailOrId, DateTime dateTime,
            CustomEventTiming timing, string timezone = "UTC");

        /// <summary>
        /// Gets the insulin medication requests for a given patient based on a date, timing, and the user's timezone.
        /// The results are limited to a single day timespan due to CustomEventTiming.
        /// </summary>
        /// <param name="patientEmailOrId">The patient's unique email or ID</param>
        /// <param name="dateTime">The date and time to get the results from</param>
        /// <param name="timing">A <see cref="CustomEventTiming"/> to limit the results to a timing in the day</param>
        /// <param name="timezone">The user's timezone. Defaults to UTC</param>
        /// <returns>A <see cref="Bundle"/> with the results</returns>
        /// <exception cref="NotFoundException">If the patient was not found.</exception>
        public Task<Bundle> ProcessInsulinMedicationRequest(string patientEmailOrId, DateTime dateTime,
            CustomEventTiming timing, string timezone = "UTC");

        /// <summary>
        /// Gets the glucose service requests for a given patient based on a date, timing, and the user's timezone.
        /// The results are limited to a single day timespan due to CustomEventTiming.
        /// </summary>
        /// <param name="patientEmailOrId">The patient's unique email or ID</param>
        /// <param name="dateTime">The date and time to get the results from</param>
        /// <param name="timing">A <see cref="CustomEventTiming"/> to limit the results to a timing in the day</param>
        /// <param name="timezone">The user's timezone. Defaults to UTC</param>
        /// <returns>A <see cref="Bundle"/> with the results</returns>
        /// <exception cref="NotFoundException">If the patient is not found.</exception>
        public Task<Bundle> ProcessGlucoseServiceRequest(string patientEmailOrId, DateTime dateTime,
            CustomEventTiming timing, string timezone = "UTC");

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
        /// <exception cref="NotFoundException">If the patient is not found.</exception>
        public Task<Bundle> ProcessCarePlanRequest(string patientEmailOrId, DateTime dateTime, CustomEventTiming timing,
            string timezone = "UTC");

        /// <summary>
        /// Gets the next requests for a patient to follow given a request type. 
        /// </summary>
        /// <param name="patientEmailOrId">The patient's ID or email who owns the requests.</param>
        /// <param name="type">The <see cref="AlexaRequestType"/></param>
        /// <returns>A <see cref="Bundle"/> object with the list of requests.</returns>
        /// <exception cref="NotFoundException">If the patient is not found.</exception>
        public Task<Bundle> GetNextRequests(string patientEmailOrId, AlexaRequestType type);

        /// <summary>
        /// Gets the next requests for a patient to follow without filtering the request type.
        /// </summary>
        /// <param name="patientEmailOrId">The patient's ID or email who owns the requests.</param>
        /// <returns>A <see cref="Bundle"/> object with the list of requests.</returns>
        /// <exception cref="NotFoundException">If the patient is not found.</exception>
        public Task<Bundle> GetNextRequests(string patientEmailOrId);

        /// <summary>
        /// Updates / Adds a specific time for a event timing to the patient's list. e.g., a specific time for breakfast.  
        /// </summary>
        /// <param name="patientIdOrEmail">The patient's ID or email.</param>
        /// <param name="eventTiming">The event timing to set.</param>
        /// <param name="dateTime">The time for the event. The date is ignored.</param>
        /// <returns>A boolean value to indicate is the update was successful.</returns>
        /// <exception cref="NotFoundException">If the patient is not found.</exception>
        /// <exception cref="UpdateException">If there is an error updating the patient's timing</exception>
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
        /// <exception cref="NotFoundException">If the patient is not found.</exception>
        /// <exception cref="UpdateException">If there is an error updating the patient's timing</exception>
        Task<bool> UpsertDosageStartDate(string patientIdOrEmail, string dosageId, DateTime startDate);
    }
}