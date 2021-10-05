namespace QMUL.DiabetesBackend.ServiceImpl.Implementations
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using DataInterfaces;
    using ServiceInterfaces;
    using Patient = Model.Patient;

    /// <summary>
    /// The Patient Service manages patients
    /// </summary>
    public class PatientService : IPatientService
    {
        private readonly IPatientDao patientDao;

        public PatientService(IPatientDao patientDao, IMedicationRequestDao medicationRequestDao)
        {
            this.patientDao = patientDao;
        }

        /// <inheritdoc/>>
        public async Task<List<Patient>> GetPatientList()
        {
            return await this.patientDao.GetPatients();
        }

        /// <inheritdoc/>>
        public async Task<Patient> CreatePatient(Patient newPatient)
        {
            return await this.patientDao.CreatePatient(newPatient);
        }

        /// <inheritdoc/>>
        public async Task<Patient> GetPatient(string idOrEmail)
        {
            var result = await this.patientDao.GetPatientByIdOrEmail(idOrEmail);
            if (result == null)
            {
                throw new KeyNotFoundException();
            }

            return result;
        }
    }
}
