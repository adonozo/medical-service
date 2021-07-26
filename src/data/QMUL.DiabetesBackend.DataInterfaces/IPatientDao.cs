using System.Collections.Generic;
using System.Threading.Tasks;
using QMUL.DiabetesBackend.Model;

namespace QMUL.DiabetesBackend.DataInterfaces
{
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
        public Task<Patient> CreatePatient(Patient newPatient);

        /// <summary>
        /// Gets a single patient identified by ID or email. Emails are unique for patients, so it would always return
        /// a single result.
        /// </summary>
        /// <param name="idOrEmail">The patient's ID or email.</param>
        /// <returns>The patient.</returns>
        public Task<Patient> GetPatientByIdOrEmail(string idOrEmail);
        
        /// <summary>
        /// Updates (replaces) a patient.
        /// </summary>
        /// <param name="actualPatient">The patient to update.</param>
        /// <returns>A boolean value to tell if the update was made.</returns>
        public Task<bool> UpdatePatient(Patient actualPatient);
    }
}