namespace QMUL.DiabetesBackend.ServiceInterfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Patient = Model.Patient;

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
        public Task<Patient> CreatePatient(Patient newPatient);

        public Task<Patient> GetPatient(string idOrEmail);
    }
}