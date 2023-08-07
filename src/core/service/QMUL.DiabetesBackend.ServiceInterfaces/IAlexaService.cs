namespace QMUL.DiabetesBackend.ServiceInterfaces;

using System.Threading.Tasks;
using Hl7.Fhir.Model;
using Model;
using Model.Enums;
using Model.Exceptions;
using NodaTime;

/// <summary>
/// The Alexa Service Interface. Manages Alexa related requests.
/// </summary>
public interface IAlexaService
{
    /// <summary>
    /// Search the active medication requests that has occurrences in the provided date and timing event. When the
    /// medication request has multiple dosages, <b>the result will contain only the dosages that match the provided
    /// date</b>
    /// <br/>
    /// If any medication request needs a start date, it will return a failed <see cref="Result{TSuccess,TError}"/> with
    /// the medication request that needs set up.
    /// </summary>
    /// <param name="patientEmailOrId">The patient's unique email or ID</param>
    /// <param name="date">The date to get the results from</param>
    /// <param name="onlyInsulin">To tell if it should filter out non-insulin medication requests</param>
    /// <param name="timing">A <see cref="CustomEventTiming"/> to limit the results to a timing in the day</param>
    /// <param name="timezone">The user's timezone. Defaults to UTC</param>
    /// <returns>A search <see cref="Bundle"/> with the matching medication results, or the medication request that
    /// needs a start date.</returns>
    Task<Result<Bundle, MedicationRequest>> SearchMedicationRequests(string patientEmailOrId,
        LocalDate date,
        bool onlyInsulin,
        CustomEventTiming? timing = CustomEventTiming.ALL_DAY,
        string? timezone = "UTC");

    /// <summary>
    /// Gets the glucose service requests for a given patient based on a date, timing, and the user's timezone.
    /// The results are limited to a single day timespan due to CustomEventTiming.
    /// </summary>
    /// <param name="patientEmailOrId">The patient's unique email or ID</param>
    /// <param name="dateTime">The date and time to get the results from</param>
    /// <param name="timing">A <see cref="CustomEventTiming"/> to limit the results to a timing in the day</param>
    /// <param name="timezone">The user's timezone. Defaults to UTC</param>
    /// <returns>A search <see cref="Bundle"/> with the matching medication results, or the service request that
    /// needs a start date.</returns>
    Task<Result<Bundle, ServiceRequest>> SearchServiceRequests(string patientEmailOrId,
        LocalDate dateTime,
        CustomEventTiming timing,
        string timezone = "UTC");

    /// <summary>
    /// Updates / Adds a specific time for a event timing to the patient's list. e.g., a specific time for breakfast.
    /// </summary>
    /// <param name="patientIdOrEmail">The patient's ID or email.</param>
    /// <param name="eventTiming">The event timing to set.</param>
    /// <param name="localTime">The time for the event</param>
    /// <returns>A boolean value to indicate is the update was successful.</returns>
    /// <exception cref="ValidationException">If the patient was not found</exception>
    /// <exception cref="WriteResourceException">If there is an error updating the patient's timing</exception>
    Task<bool> UpsertTimingEvent(string patientIdOrEmail, CustomEventTiming eventTiming, LocalTime localTime);

    /// <summary>
    /// Updates / Adds a start date for a dosage instruction. Useful when the medication doesn't have an exact start
    /// date and it has a duration rather than a period, e.g., for 7 days, for a month, etc.
    /// The associated health events are updated as well by deleting old events and creating new ones.
    /// </summary>
    /// <param name="patientIdOrEmail">The patient's ID or email.</param>
    /// <param name="dosageId">The dosage ID to update.</param>
    /// <param name="startDate">The start date.</param>
    /// <param name="localTime">The optional start time</param>
    /// <returns>A boolean value to indicate is the update was successful.</returns>
    /// <exception cref="ValidationException">If the patient is not found.</exception>
    /// <exception cref="WriteResourceException">If there is an error updating the patient's timing</exception>
    Task<bool> UpsertDosageStartDateTime(string patientIdOrEmail,
        string dosageId,
        LocalDate startDate,
        LocalTime? localTime = null);

    /// <summary>
    /// Updates / Adds a start date for a service request. Useful when the service request doesn't have an exact
    /// start date and it has a duration rather than a period, e.g., for 7 days, for a month, etc.
    /// The associated health events are updated as well by deleting old events and creating new ones.
    /// </summary>
    /// <param name="patientIdOrEmail">The patient's ID or email.</param>
    /// <param name="serviceRequestId">The service request ID to update.</param>
    /// <param name="startDate">The start date.</param>
    /// <returns>A boolean value to indicate is the update was successful.</returns>
    /// <exception cref="ValidationException">If the patient was not found</exception>
    Task<bool> UpsertServiceRequestStartDate(string patientIdOrEmail, string serviceRequestId, LocalDate startDate);
}