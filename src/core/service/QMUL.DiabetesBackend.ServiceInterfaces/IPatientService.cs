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
        public Task<List<Patient>> GetPatientList();

        public Task<Patient> CreatePatient(Patient newPatient);

        public Task<Patient> GetPatient(string idOrEmail);
    }
}