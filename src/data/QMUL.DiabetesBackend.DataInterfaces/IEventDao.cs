namespace QMUL.DiabetesBackend.DataInterfaces;

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