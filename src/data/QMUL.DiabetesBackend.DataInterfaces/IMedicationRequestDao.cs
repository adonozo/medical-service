namespace QMUL.DiabetesBackend.DataInterfaces;

using System.Collections.Generic;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using Model;
using Model.Exceptions;
using Task = System.Threading.Tasks.Task;

/// <summary>
/// The Medication Request Dao interface.
/// </summary>
public interface IMedicationRequestDao
{
    /// <summary>
    /// Creates the medication request from the argument.
    /// </summary>
    /// <param name="newRequest">The <see cref="MedicationRequest"/> to create.</param>
    /// <returns>The newly created medication request.</returns>
    /// <exception cref="WriteResourceException">If the medication was not inserted or found after being inserted.</exception>
    Task<MedicationRequest> CreateMedicationRequest(MedicationRequest newRequest);

    /// <summary>
    /// Updates a medication request.
    /// </summary>
    /// <param name="id">The medication request's ID</param>
    /// <param name="actualRequest">The <see cref="MedicationRequest"/> to update</param>
    /// <returns>The updated medication request</returns>
    /// <exception cref="WriteResourceException">If the medication request was not updated or found after the update.</exception>
    Task<bool> UpdateMedicationRequest(string id, MedicationRequest actualRequest);

    /// <summary>
    /// Gets a <see cref="MedicationRequest"/> based on the ID.
    /// </summary>
    /// <param name="id">The medication request's ID.</param>
    /// <returns>The medication request found.</returns>
    Task<MedicationRequest?> GetMedicationRequest(string id);

    /// <summary>
    /// Gets a list of medication requests matching an array of medication IDs.
    /// </summary>
    /// <param name="ids">The medication request IDs array.</param>
    /// <returns>A list of <see cref="Medication"/>.</returns>
    Task<IList<MedicationRequest>> GetMedicationRequestsByIds(string[] ids);

    /// <summary>
    /// Gets all the medication requests for a given patient ID.
    /// </summary>
    /// <param name="patientId">The patient's ID.</param>
    /// <returns>The patient's list of <see cref="MedicationRequest"/>.</returns>
    Task<IList<MedicationRequest>> GetMedicationRequestFor(string patientId);

    /// <summary>
    /// Deletes a medication request given an ID.
    /// </summary>
    /// <param name="id">The medication request ID.</param>
    /// <returns>True if the medication request was deleted. False if it was not deleted or it does not exist.</returns>
    Task<bool> DeleteMedicationRequest(string id);

    /// <summary>
    /// Gets the medication request that holds a given Dosage ID for a patient.
    /// </summary>
    /// <param name="patientId">The patient ID.</param>
    /// <param name="dosageId">The dosage ID.</param>
    /// <returns>The <see cref="MedicationRequest"/> that contains the Dosage reference.</returns>
    Task<MedicationRequest?> GetMedicationRequestForDosage(string patientId, string dosageId);

    /// <summary>
    /// Gets the active medication requests for the patient, i.e., the ones that the patient needs to follow. Does
    /// not include insulin request
    /// </summary>
    /// <param name="patientId">The patient's user ID, not email</param>
    /// <param name="paginationRequest">The pagination request parameter.</param>
    /// <returns>The list of active medication requests within a <see cref="PaginatedResult{T}"/> object.</returns>
    Task<PaginatedResult<IEnumerable<Resource>>> GetActiveMedicationRequests(string patientId,
        PaginationRequest paginationRequest);

    /// <summary>
    /// Gets the all the active medication requests, insulin and non-insulin.
    /// </summary>
    /// <param name="patientId">The patient's user ID (not email)</param>
    /// <returns>The list of active medication requests.</returns>
    Task<IList<MedicationRequest>> GetAllActiveMedicationRequests(string patientId);
}