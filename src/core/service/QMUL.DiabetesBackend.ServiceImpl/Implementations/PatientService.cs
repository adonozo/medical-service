namespace QMUL.DiabetesBackend.ServiceImpl.Implementations
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using DataInterfaces;
    using ServiceInterfaces;
    using Microsoft.Extensions.Logging;
    using Utils;
    using Patient = Model.Patient;

    /// <summary>
    /// The Patient Service manages patients
    /// </summary>
    public class PatientService : IPatientService
    {
        private readonly IPatientDao patientDao;
        private readonly ILogger<PatientService> logger;

        public PatientService(IPatientDao patientDao, ILogger<PatientService> logger)
        {
            this.patientDao = patientDao;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public async Task<List<Patient>> GetPatientList()
        {
            return await this.patientDao.GetPatients();
        }

        /// <inheritdoc/>
        public async Task<Patient> CreatePatient(Patient newPatient)
        {
            this.logger.LogDebug("Creating new patient {Email}", newPatient.Email);
            return await ExceptionHandler.ExecuteAndHandleAsync(async () =>
                await this.patientDao.CreatePatient(newPatient), this.logger);
        }

        /// <inheritdoc/>
        public async Task<Patient> GetPatient(string idOrEmail)
        {
            var patient = await ExceptionHandler.ExecuteAndHandleAsync(async () =>
                await this.patientDao.GetPatientByIdOrEmail(idOrEmail), this.logger);
            this.logger.LogDebug("Patient found: {IdOrEmail}", idOrEmail);
            return patient;
        }
    }
}