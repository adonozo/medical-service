namespace QMUL.DiabetesBackend.DataInterfaces;

using System.Collections.Generic;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using Model;
using Model.Exceptions;
using Instant = NodaTime.Instant;

/// <summary>
/// The Observation Dao interface.
/// </summary>
public interface IObservationDao
{
    /// <summary>
    /// Creates a given <see cref="Observation"/>. It generates an ID.
    /// </summary>
    /// <param name="observation">The <see cref="Observation"/> to insert to the Database</param>
    /// <returns>The inserted <see cref="Observation"/> with a new ID.</returns>
    /// <exception cref="WriteResourceException">If the observation is not created.</exception>
    Task<Observation> CreateObservation(Observation observation);

    /// <summary>
    /// Gets a single observation given an ID.
    /// </summary>
    /// <param name="id">The observation ID to look for.</param>
    /// <returns>A <see cref="Observation"/></returns>
    Task<Observation?> GetObservation(string id);

    /// <summary>
    /// Gets all the observations for a given patient.
    /// </summary>
    /// <param name="patientId">The patient ID.</param>
    /// <param name="paginationRequest">The paginated request parameters.</param>
    /// <returns>The patient's list of <see cref="Observation"/> in a paginated <see cref="PaginatedResult{T}"/>
    /// object.</returns>
    Task<PaginatedResult<IEnumerable<Resource>>> GetAllObservationsFor(string patientId,
        PaginationRequest paginationRequest);

    /// <summary>
    /// Gets the list of <see cref="Observation"/> for a given patient in a defined time range.
    /// </summary>
    /// <param name="patientId">The patient ID.</param>
    /// <param name="paginationRequest">The paginated request parameters.</param>
    /// <param name="start">The range start datetime.</param>
    /// <param name="end">The range end datetime.</param>
    /// <returns>An <see cref="Observation"/> list within the start and end dates; contained within a paginated
    /// <see cref="PaginatedResult{T}"/> object.</returns>
    Task<PaginatedResult<IEnumerable<Resource>>> GetObservationsFor(string patientId,
        PaginationRequest paginationRequest,
        Instant? start = null,
        Instant? end = null);

    /// <summary>
    /// Updates a given <see cref="Observation"/>.
    /// </summary>
    /// <param name="id">The observation ID.</param>
    /// <param name="observation">The <see cref="Observation"/> to update.</param>
    /// <returns>A bool value indicating the operation result</returns>
    Task<bool> UpdateObservation(string id, Observation observation);

    /// <summary>
    /// Deletes an <see cref="Observation"/> given an ID.
    /// </summary>
    /// <param name="id">The observation ID to delete.</param>
    /// <returns>True if the observation was deleted. False otherwise</returns>
    Task<bool> DeleteObservation(string id);
}