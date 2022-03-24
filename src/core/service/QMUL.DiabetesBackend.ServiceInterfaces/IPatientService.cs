namespace QMUL.DiabetesBackend.ServiceInterfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Exceptions;
    using Hl7.Fhir.Model;
    using Model;

    /// <summary>
    /// The Patient Service Interface.
    /// </summary>
    public interface IPatientService
    {
        /// <summary>
        /// Gets the list of all patients.
        /// </summary>
        /// <returns>A <see cref="List{T}"/> of all registered patients.</returns>
        public Task<List<Patient>> GetPatientList();

        /// <summary>
        /// Creates a <see cref="Patient"/>.
        /// </summary>
        /// <param name="newPatient">The new <see cref="Patient"/> to create.</param>
        /// <returns>The created <see cref="Patient"/> with a new ID.</returns>
        /// <exception cref="CreateException">If the patient could not be created.</exception>
        public Task<Patient> CreatePatient(Patient newPatient);

        /// <summary>
        /// Gets a single patient given an Id or email.
        /// </summary>
        /// <param name="idOrEmail">The patient's ID or email</param>
        /// <returns>The <see cref="Patient"/></returns>
        /// <exception cref="NotFoundException">If the patient was not found.</exception>
        public Task<Patient> GetPatient(string idOrEmail);

        /// <summary>
        /// Updates a patient given an ID and an updated patient object by replacing the patient in the DB.
        /// </summary>
        /// <param name="idOrEmail">The patient's ID or email.</param>
        /// <param name="updatedPatient">The patient object to update.</param>
        /// <returns>The updated <see cref="Patient"/>.</returns>
        /// <exception cref="NotFoundException">If the patient was not found.</exception>
        /// <exception cref="UpdateException">If there was an error during update</exception>
        public Task<Patient> UpdatePatient(string idOrEmail, Patient updatedPatient);
        
        /// <summary>
        /// Updates a patient's fields that are not empty or default.
        /// </summary>
        /// <param name="idOrEmail">The patient's ID or email.</param>
        /// <param name="updatedPatient">The patient object to update. Any null or empty field will be ignored.</param>
        /// <returns>The updated <see cref="Patient"/>.</returns>
        /// <exception cref="NotFoundException">If the patient was not found.</exception>
        /// <exception cref="UpdateException">If there was an error during update</exception>
        public Task<Patient> PatchPatient(string idOrEmail, InternalPatient updatedPatient);
    }
}