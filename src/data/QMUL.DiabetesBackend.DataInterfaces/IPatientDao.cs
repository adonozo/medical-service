namespace QMUL.DiabetesBackend.DataInterfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Exceptions;
    using Model;

    /// <summary>
    /// The Patient Dao interface.
    /// </summary>
    public interface IPatientDao
    {
        /// <summary>
        /// The entire list of patients
        /// </summary>
        /// <returns>A list containing all registered patients</returns>
        public Task<List<Patient>> GetPatients();

        /// <summary>
        /// Creates a patient.
        /// </summary>
        /// <param name="newPatient">The patient to create.</param>
        /// <returns>The created patient.</returns>
        /// <exception cref="CreateException">If the patient could not be created.</exception>
        public Task<Patient> CreatePatient(Patient newPatient);

        /// <summary>
        /// Gets a single patient identified by ID or email. Emails are unique for patients, so it would always return
        /// a single result.
        /// </summary>
        /// <param name="idOrEmail">The patient's ID or email.</param>
        /// <returns>The patient.</returns>
        /// <exception cref="NotFoundException">If the patient is not found.</exception>
        public Task<Patient> GetPatientByIdOrEmail(string idOrEmail);

        /// <summary>
        /// Updates (replaces) a patient.
        /// </summary>
        /// <param name="actualPatient">The patient to update.</param>
        /// <returns>A boolean value to tell if the update was made.</returns>
        /// <exception cref="UpdateException">If the patient could not be updated</exception>
        public Task<Patient> UpdatePatient(Patient actualPatient);
    }
}