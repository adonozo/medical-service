namespace QMUL.DiabetesBackend.ServiceInterfaces;

using System.Threading.Tasks;
using Hl7.Fhir.Model;
using Model;
using Model.Exceptions;

/// <summary>
/// The Medication Request Service Interface.
/// </summary>
public interface IMedicationRequestService
{
    /// <summary>
    /// Creates the medication request from the argument. It must be linked to a patient by the Subject.ElementId
    /// field.
    /// </summary>
    /// <param name="request">The <see cref="MedicationRequest"/> to create.</param>
    /// <returns>The <see cref="MedicationRequest"/> with an updated ID.</returns>
    /// <exception cref="ValidationException">If the linked patient was not found.</exception>
    /// <exception cref="WriteResourceException">If the medication request or the related events could not be created.</exception>
    Task<MedicationRequest> CreateMedicationRequest(MedicationRequest request);

    /// <summary>
    /// Gets a single medication request given an ID.
    /// </summary>
    /// <param name="id">The <see cref="MedicationRequest"/> ID to look for.</param>
    /// <returns>The <see cref="MedicationRequest"/> if found; null otherwise.</returns>
    Task<MedicationRequest?> GetMedicationRequest(string id);

    /// <summary>
    /// Updates a medication request; the ID cannot be updated. The method validates that the ID is valid before
    /// attempting to update the medication request.
    /// </summary>
    /// <param name="id">The medication request's ID to be updated.</param>
    /// <param name="request">The actual <see cref="MedicationRequest"/> to save.</param>
    /// <returns>The updated <see cref="MedicationRequest"/> if found and updated. An error otherwise.</returns>
    /// <exception cref="ValidationException">If the medication request was not found.</exception>
    /// <exception cref="WriteResourceException">If the medication request could not be updated.</exception>
    Task<bool> UpdateMedicationRequest(string id, MedicationRequest request);

    /// <summary>
    /// Activates a list of medication requests
    /// </summary>
    /// <param name="ids">The medication requests IDs</param>
    /// <returns>A boolean, true if the operation was successful.</returns>
    Task<bool> ActivateMedicationRequestsStatus(string[] ids);

    /// <summary>
    /// Revokes a list of medication requests
    /// </summary>
    /// <param name="ids">The medication requests IDs</param>
    /// <returns>A boolean, true if the operation was successful.</returns>
    Task<bool> RevokeMedicationRequestsStatus(string[] ids);

    /// <summary>
    /// Deletes a medication Request given an ID.
    /// </summary>
    /// <param name="id">The medication request's ID to delete.</param>
    /// <returns>A boolean, true if the operation was successful.</returns>
    /// <exception cref="NotFoundException">If the medication request was not found.</exception>
    Task<bool> DeleteMedicationRequest(string id);

    /// <summary>
    /// Gets the active medication requests for a given patient.
    /// </summary>
    /// <param name="patientIdOrEmail">The patient's ID or email.</param>
    /// <param name="paginationRequest">The pagination request parameter.</param>
    /// <returns>A paginated <see cref="PaginatedResult{T}"/> <see cref="Bundle"/> object with the list of active
    /// medication requests.</returns>
    /// <exception cref="NotFoundException">If the patient was not found.</exception>
    Task<PaginatedResult<Bundle>> GetActiveMedicationRequests(string patientIdOrEmail,
        PaginationRequest paginationRequest);
}