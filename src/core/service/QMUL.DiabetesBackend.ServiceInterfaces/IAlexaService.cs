namespace QMUL.DiabetesBackend.ServiceInterfaces;

using System;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using Model;
using Model.Alexa;
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
    /// needs a start date</returns>
    [Obsolete("CustomEventTimings are not flexible and error prone. Use GetActiveSearchRequests instead")]
    Task<Result<Bundle, ServiceRequest>> SearchServiceRequests(string patientEmailOrId,
        LocalDate dateTime,
        CustomEventTiming timing,
        string timezone = "UTC");

    /// <summary>
    /// Searches the service requests that occur within the specified interval. If any active service request needs a
    /// start date, the result will fail
    /// </summary>
    /// <param name="patientEmailOrId">The patient's unique email or ID</param>
    /// <param name="startDate">The start date interval</param>
    /// <param name="endDate">Then end date interval</param>
    /// <returns>A search <see cref="Bundle"/> with the service requests that occur within the interval, or the service
    /// request that needs a start date</returns>
    Task<Result<Bundle, ServiceRequest>> GetActiveSearchRequests(string patientEmailOrId,
        LocalDate? startDate = null,
        LocalDate? endDate = null);

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

    /// <summary>
    /// Gets the last Alexa request that a patient has made. A call to this method is recorded as an Alexa Request.
    /// </summary>
    /// <param name="patientIdOrEmail">The patient's ID or email.</param>
    /// <param name="deviceId">The patient's Alexa device ID</param>
    /// <returns>The last <see cref="AlexaRequest"/> for the patient, or null if the patient never made a reqest before</returns>
    Task<Result<AlexaRequest?, string>> GetLastRequest(string patientIdOrEmail, string deviceId);
}