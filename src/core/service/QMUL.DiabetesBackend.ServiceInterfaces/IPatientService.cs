namespace QMUL.DiabetesBackend.ServiceInterfaces;

using System.Threading.Tasks;
using Hl7.Fhir.Model;
using Model;
using Model.Exceptions;

/// <summary>
/// The Patient Service Interface.
/// </summary>
public interface IPatientService
{
    /// <summary>
    /// Gets a list of <see cref="Patient"/> contained within a <see cref="Bundle"/> and paginated.
    /// </summary>
    /// <param name="paginationRequest">The pagination request parameter.</param>
    /// <returns>A <see cref="PaginatedResult{T}"/> with the patient's <see cref="Bundle"/>.</returns>
    Task<PaginatedResult<Bundle>> GetPatientList(PaginationRequest paginationRequest);

    /// <summary>
    /// Creates a <see cref="Patient"/>.
    /// </summary>
    /// <param name="newPatient">The new <see cref="Patient"/> to create.</param>
    /// <returns>The created <see cref="Patient"/> with a new ID.</returns>
    /// <exception cref="WriteResourceException">If the patient could not be created.</exception>
    Task<Patient> CreatePatient(Patient newPatient);

    /// <summary>
    /// Gets a single patient given an Id or email.
    /// </summary>
    /// <param name="idOrEmail">The patient's ID or email</param>
    /// <returns>The <see cref="Patient"/>, or null if it was not found.</returns>
    Task<Patient?> GetPatient(string idOrEmail);

    /// <summary>
    /// Updates a patient given an ID and an updated patient object by replacing the patient in the DB.
    /// </summary>
    /// <param name="idOrEmail">The patient's ID or email.</param>
    /// <param name="updatedPatient">The patient object to update.</param>
    /// <returns>The updated <see cref="Patient"/>.</returns>
    /// <exception cref="NotFoundException">If the patient was not found.</exception>
    /// <exception cref="WriteResourceException">If there was an error during update</exception>
    Task<Patient> UpdatePatient(string idOrEmail, Patient updatedPatient);

    /// <summary>
    /// Updates a patient's fields that are not empty or default.
    /// </summary>
    /// <param name="idOrEmail">The patient's ID or email.</param>
    /// <param name="updatedPatient">The patient object to update. Any null or empty field will be ignored.</param>
    /// <returns>The updated <see cref="Patient"/>.</returns>
    /// <exception cref="NotFoundException">If the patient was not found.</exception>
    /// <exception cref="WriteResourceException">If there was an error during update</exception>
    Task<Patient> PatchPatient(string idOrEmail, InternalPatient updatedPatient);
}