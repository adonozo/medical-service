namespace QMUL.DiabetesBackend.DataInterfaces;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Model;
using Model.Enums;

/// <summary>
/// The Event Dao Interface
/// </summary>
public interface IEventDao
{
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