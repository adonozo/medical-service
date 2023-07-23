namespace QMUL.DiabetesBackend.ServiceInterfaces;

using System;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using Model.Enums;
using Model.Exceptions;

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
    /// <param name="onlyInsulin">To tell if it should filter out non-insulin medication requests</param>
    /// <param name="timing">A <see cref="CustomEventTiming"/> to limit the results to a timing in the day</param>
    /// <param name="timezone">The user's timezone. Defaults to UTC</param>
    /// <returns>A <see cref="Bundle"/> with the results, or null if the patient was not found.</returns>
    Task<Bundle> SearchMedicationRequests(string patientEmailOrId,
        DateTime dateTime,
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
    /// <returns>A <see cref="Bundle"/> with the results, or null if the patient was not found.</returns>
    Task<Bundle?> ProcessGlucoseServiceRequest(string patientEmailOrId, DateTime dateTime,
        CustomEventTiming timing, string timezone = "UTC");

    /// <summary>
    /// Updates / Adds a specific time for a event timing to the patient's list. e.g., a specific time for breakfast.  
    /// </summary>
    /// <param name="patientIdOrEmail">The patient's ID or email.</param>
    /// <param name="eventTiming">The event timing to set.</param>
    /// <param name="dateTime">The time for the event. The date is ignored.</param>
    /// <returns>A boolean value to indicate is the update was successful.</returns>
    /// <exception cref="ValidationException">If the patient was not found</exception>
    /// <exception cref="WriteResourceException">If there is an error updating the patient's timing</exception>
    Task<bool> UpsertTimingEvent(string patientIdOrEmail, CustomEventTiming eventTiming, DateTime dateTime);

    /// <summary>
    /// Updates / Adds a start date for a dosage instruction. Useful when the medication doesn't have an exact start
    /// date and it has a duration rather than a period, e.g., for 7 days, for a month, etc.
    /// The associated health events are updated as well by deleting old events and creating new ones. 
    /// </summary>
    /// <param name="patientIdOrEmail">The patient's ID or email.</param>
    /// <param name="dosageId">The dosage ID to update.</param>
    /// <param name="startDate">The start date.</param>
    /// <returns>A boolean value to indicate is the update was successful.</returns>
    /// <exception cref="ValidationException">If the patient is not found.</exception>
    /// <exception cref="WriteResourceException">If there is an error updating the patient's timing</exception>
    Task<bool> UpsertDosageStartDate(string patientIdOrEmail, string dosageId, DateTime startDate);

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
    Task<bool> UpsertServiceRequestStartDate(string patientIdOrEmail, string serviceRequestId, DateTime startDate);
}