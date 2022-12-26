namespace QMUL.DiabetesBackend.DataInterfaces;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Model;
using Model.Enums;
using Model.Exceptions;

/// <summary>
/// The Event Dao Interface
/// </summary>
public interface IEventDao
{
    /// <summary>
    /// Creates a series of <see cref="HealthEvent"/> 
    /// </summary>
    /// <param name="events">The list of <see cref="HealthEvent"/> to insert.</param>
    /// <returns>A boolean value to indicate if the list was inserted.</returns>
    Task<bool> CreateEvents(IEnumerable<HealthEvent> events);

    Task<bool> DeleteAllRelatedResources(string[] resourceIds);

    /// <summary>
    /// Deletes all events related to a resource, i.e., Medication or Service requests. Should be called to tidy up
    /// resources. 
    /// </summary>
    /// <param name="resourceId">Medication or Service request ID</param>
    /// <returns>A bool to indicate if the operation was successful</returns>
    Task<bool> DeleteRelatedEvents(string resourceId);

    /// <summary>
    /// Deletes the event series based on a reference ID.
    /// </summary>
    /// <param name="referenceId">The Reference ID. i.e., the dosageInstruction ID from the medication request.</param>
    /// <returns>A bool to indicate if the delete operation was successful.</returns>
    Task<bool> DeleteEventSeries(string referenceId);

    /// <summary>
    /// Updates all events' times that matches the timing. Only newer events. 
    /// </summary>
    /// <param name="patientId">The patient ID</param>
    /// <param name="timing">The timing to match</param>
    /// <param name="time">The time to change. Date is ignored</param>
    /// <returns>A boolean value to indicate if the update was successful.</returns>
    /// <exception cref="WriteResourceException">If any of the events were not updated.</exception>
    Task<bool> UpdateEventsTiming(string patientId, CustomEventTiming timing, DateTimeOffset time);

    /// <summary>
    /// Gets health events for the given parameters.
    /// </summary>
    /// <param name="patientId">The patient ID.</param>
    /// <param name="type">The <see cref="EventType"/></param>
    /// <param name="start">The range start datetime</param>
    /// <param name="end">The range end datetime to look for</param>
    /// <returns>A list of <see cref="HealthEvent"/> matching the parameters.</returns>
    Task<IEnumerable<HealthEvent>> GetEvents(string patientId, EventType type, DateTime start, DateTime end);

    /// <summary>
    /// Gets health events for the given parameters.
    /// </summary>
    /// <param name="patientId">The patient ID.</param>
    /// <param name="type">The <see cref="EventType"/></param>
    /// <param name="start">The range start datetime</param>
    /// <param name="end">The range end datetime to look for</param>
    /// <param name="timings">An array of <see cref="CustomEventTiming"/> to look for</param>
    /// <returns>A list of <see cref="HealthEvent"/> matching the parameters.</returns>
    Task<IEnumerable<HealthEvent>> GetEvents(string patientId, EventType type, DateTime start, DateTime end,
        CustomEventTiming[] timings);

    /// <summary>
    /// Gets health events for the given parameters.
    /// </summary>
    /// <param name="patientId">The patient ID.</param>
    /// <param name="types">An array of <see cref="EventType"/> to look for.</param>
    /// <param name="start">The range start datetime</param>
    /// <param name="end">The range end datetime to look for</param>
    /// <returns>A list of <see cref="HealthEvent"/> matching the parameters.</returns>
    Task<IEnumerable<HealthEvent>> GetEvents(string patientId, EventType[] types, DateTime start,
        DateTime end);

    /// <summary>
    /// Gets health events for the given parameters.
    /// </summary>
    /// <param name="patientId">The patient ID.</param>
    /// <param name="types">An array of <see cref="EventType"/> to look for.</param>
    /// <param name="start">The range start datetime.</param>
    /// <param name="end">The range end datetime to look for.</param>
    /// <param name="timings">An array of <see cref="CustomEventTiming"/> to look for.</param>
    /// <returns>A list of <see cref="HealthEvent"/> matching the parameters.</returns>
    Task<IEnumerable<HealthEvent>> GetEvents(string patientId, EventType[] types, DateTime start,
        DateTime end, CustomEventTiming[] timings);

    /// <summary>
    /// Gets the next batch of events for a given patient. There is a default limit for the results.
    /// </summary>
    /// <param name="patientId">The patient ID.</param>
    /// <param name="type">The event type to look for.</param>
    /// <returns>A list of <see cref="HealthEvent"/> matching the parameters.</returns>
    Task<IEnumerable<HealthEvent>> GetNextEvents(string patientId, EventType type);

    /// <summary>
    /// Gets the next batch of events for a given patient. There is a default limit for the results.
    /// </summary>
    /// <param name="patientId">The patient ID.</param>
    /// <param name="types">An array of <see cref="EventType"/> to look for.</param>
    /// <returns>A list of <see cref="HealthEvent"/> matching the parameters.</returns>
    Task<IEnumerable<HealthEvent>> GetNextEvents(string patientId, EventType[] types);
}