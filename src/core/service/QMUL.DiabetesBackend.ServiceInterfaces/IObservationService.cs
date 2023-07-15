namespace QMUL.DiabetesBackend.ServiceInterfaces;

using System;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using Model;
using Model.Enums;
using Model.Exceptions;

/// <summary>
/// The Observation Service Interface. Observations are glucose self-measurements for a patient. 
/// </summary>
public interface IObservationService
{
    /// <summary>
    /// Creates an <see cref="Observation"/>. If the patientId is present, it will be validated against an existing
    /// patient and will override the Subject. If not, the Subject will be validated.
    /// </summary>
    /// <param name="patientId">The patient's ID or email.</param>
    /// <param name="newObservation">The <see cref="Observation"/> to create.</param>
    /// <returns>The <see cref="Observation"/> created with a new ID.</returns>
    /// <exception cref="ValidationException">If the patient was not found.</exception>
    /// <exception cref="WriteResourceException">If the observation could not be created.</exception>
    Task<Observation> CreateObservation(Observation newObservation, string? patientId = null);

    /// <summary>
    /// Gets a single <see cref="Observation"/>.
    /// </summary>
    /// <param name="observationId">The observation ID to look for.</param>
    /// <returns>A single <see cref="Observation"/> object if found; null otherwise.</returns>
    Task<Observation?> GetObservation(string observationId);

    /// <summary>
    /// Gets all the observations for a given patient.
    /// </summary>
    /// <param name="patientId">The patient ID who owns the observations.</param>
    /// <param name="paginationRequest">The paginated request parameters.</param>
    /// <returns>The list of <see cref="Observation"/> for a patient in a paginated <see cref="Bundle"/> object.</returns>
    Task<PaginatedResult<Bundle>> GetObservations(string patientId, PaginationRequest paginationRequest);

    /// <summary>
    /// Gets the <see cref="Observation"/> for a patient given a <see cref="CustomEventTiming"/> event and a datetime.
    /// It considers the patient's timezone (as a string) to calculate the time interval.
    /// </summary>
    /// <param name="patientId">The patient's ID or email who owns the observations.</param>
    /// <param name="timing">A <see cref="CustomEventTiming"/> where the observation(s) happened.</param>
    /// <param name="dateTime">The <see cref="DateTime"/> when the observation(s) happened.</param>
    /// <param name="paginationRequest">The paginated request parameters.</param>
    /// <param name="patientTimezone">The patient's timezone.</param>
    /// <returns>A <see cref="PaginatedResult{T}"/> <see cref="Bundle"/> object with the list of observations.</returns>
    /// <exception cref="NotFoundException">If the patient was not found.</exception>
    Task<PaginatedResult<Bundle>> GetObservationsFor(string patientId,
        CustomEventTiming timing,
        DateTime dateTime,
        PaginationRequest paginationRequest,
        string patientTimezone = "UTC");

    /// <summary>
    /// Updates an existing <see cref="Observation"/> with the object.
    /// </summary>
    /// <param name="id">The observation ID.</param>
    /// <param name="updatedObservation">The updated <see cref="Observation"/> to insert.</param>
    /// <returns>A bool value indicating the operation result</returns>
    /// <exception cref="NotFoundException">If the observation was not found.</exception>
    Task<bool> UpdateObservation(string id, Observation updatedObservation);

    /// <summary>
    /// Updates the value of an <see cref="Observation"/>.
    /// </summary>
    /// <param name="observationId">The observation's ID to update.</param>
    /// <param name="value">The new value.</param>
    /// <returns>A bool value indicating the operation result</returns>
    /// <exception cref="NotFoundException">If the observation was not found.</exception>
    Task<bool> UpdateValue(string observationId, DataType value);

    /// <summary>
    /// Deletes an <see cref="Observation"/> given an observation ID.
    /// </summary>
    /// <param name="id">The observation ID to delete.</param>
    /// <returns>A boolean, true if the operation was successful.</returns>
    /// <exception cref="NotFoundException">If the observation was not found.</exception>
    Task<bool> DeleteObservation(string id);
}