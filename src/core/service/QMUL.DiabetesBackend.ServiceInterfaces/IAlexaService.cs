using System;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using QMUL.DiabetesBackend.Model.Enums;

namespace QMUL.DiabetesBackend.ServiceInterfaces
{
    public interface IAlexaService
    {
        public Task<Bundle> ProcessRequest(string patientEmailOrId, AlexaRequestType type, DateTime dateTime,
            AlexaRequestTime requestTime, CustomEventTiming timing);

        /// <summary>
        /// Gets the medication requests for a given time.
        /// </summary>
        /// <returns>A Bundle list containing <see cref="MedicationRequest"/></returns>
        public Task<Bundle> GetMedicationRequests(string patientEmailOrId, DateTime dateTime,
            AlexaRequestTime requestTime, CustomEventTiming timing, bool insulin);

        /// <summary>
        /// Gets the service requests for a given time.
        /// </summary>
        /// /// <returns>A Bundle list containing <see cref="ServiceRequest"/></returns>
        public Task<Bundle> GetServiceRequests(string patientEmailOrId, DateTime dateTime,
            AlexaRequestTime requestTime, CustomEventTiming timing);

        /// <summary>
        /// Gets the patient's blood glucose measures for a given time 
        /// </summary>
        /// <returns>A Bundle list containing <see cref="Measure"/></returns>
        public Task<Bundle> GetMeasurements(string patientEmailOrId, DateTime dateTime,
            AlexaRequestTime requestTime, CustomEventTiming timing);

        public DiagnosticReport SaveGlucoseMeasure(string patientId, DiagnosticReport report);
        
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
        /// date; has a duration rather than a period, e.g., for 7 days, for a month, etc. 
        /// </summary>
        /// <param name="patientIdOrEmail">The patient's ID or email.</param>
        /// <param name="dosageId">The dosage ID to update.</param>
        /// <param name="startDate">The start date.</param>
        /// <returns>A boolean value to indicate is the update was successful.</returns>
        Task<bool> UpsertDosageStartDate(string patientIdOrEmail, string dosageId, DateTime startDate);
    }
}