namespace QMUL.DiabetesBackend.DataInterfaces;

using System.Collections.Generic;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using Model;
using Model.Exceptions;

/// <summary>
/// The Patient Dao interface.
/// </summary>
public interface IPatientDao
{
    /// <summary>
    /// Gets a list of paginated <see cref="Patient"/>.
    /// </summary>
    /// <returns>A <see cref="PaginatedResult{T}"/> with the patients.</returns>
    Task<PaginatedResult<IEnumerable<Resource>>> GetPatients(PaginationRequest paginationRequest);

    /// <summary>
    /// Creates a patient.
    /// </summary>
    /// <param name="newPatient">The patient to create.</param>
    /// <returns>The created patient.</returns>
    /// <exception cref="WriteResourceException">If the patient could not be created.</exception>
    Task<Patient> CreatePatient(Patient newPatient);

    /// <summary>
    /// Gets a single patient identified by ID or email. Emails are unique for patients, so it would always return
    /// a single result.
    /// </summary>
    /// <param name="idOrEmail">The patient's ID or email.</param>
    /// <returns>The patient or null if the patient does not exist</returns>
    Task<Patient?> GetPatientByIdOrEmail(string idOrEmail);

    /// <summary>
    /// Updates (replaces) a patient.
    /// </summary>
    /// <param name="actualPatient">The patient to update.</param>
    /// <returns>A bool indicating the result.</returns>
    Task<bool> UpdatePatient(Patient actualPatient);

    /// <summary>
    /// Updates only non-empty patient fields without considering custom times or the ID. 
    /// </summary>
    /// <param name="actualPatient">The patient to update. If any of the fields is empty or default, they will
    ///     be ignored</param>
    /// <param name="oldPatient">The old patient data to update.</param>
    /// <returns>A bool indicating the result.</returns>
    Task<bool> PatchPatient(InternalPatient actualPatient, Patient oldPatient);
}