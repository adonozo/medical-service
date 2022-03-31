namespace QMUL.DiabetesBackend.ServiceImpl.Implementations
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using DataInterfaces;
    using Hl7.Fhir.Model;
    using ServiceInterfaces;
    using Microsoft.Extensions.Logging;
    using Model;
    using Utils;

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
        public async Task<Bundle> GetPatientList()
        {
            var patients = await this.patientDao.GetPatients();
            return ResourceUtils.GenerateSearchBundle(patients);
        }

        /// <inheritdoc/>
        public async Task<Patient> CreatePatient(Patient newInternalPatient)
        {
            this.logger.LogDebug("Creating new patient");
            return await ExceptionHandler.ExecuteAndHandleAsync(async () =>
                await this.patientDao.CreatePatient(newInternalPatient), this.logger);
        }

        /// <inheritdoc/>
        public async Task<Patient> GetPatient(string idOrEmail)
        {
            var patient = await ExceptionHandler.ExecuteAndHandleAsync(async () =>
                await this.patientDao.GetPatientByIdOrEmail(idOrEmail), this.logger);
            this.logger.LogDebug("Patient found: {IdOrEmail}", idOrEmail);
            return patient;
        }

        /// <inheritdoc/>
        public async Task<Patient> UpdatePatient(string idOrEmail, Patient updatedInternalPatient)
        {
            await ExceptionHandler.ExecuteAndHandleAsync(async () =>
                await this.patientDao.GetPatientByIdOrEmail(idOrEmail), this.logger);
            updatedInternalPatient.Id = idOrEmail;
            return await ExceptionHandler.ExecuteAndHandleAsync(async () =>
                await this.patientDao.UpdatePatient(updatedInternalPatient), this.logger);
        }

        /// <inheritdoc/>
        public async Task<Patient> PatchPatient(string idOrEmail, InternalPatient updatedInternalPatient)
        {
            var oldPatient = await ExceptionHandler.ExecuteAndHandleAsync(async () =>
                await this.patientDao.GetPatientByIdOrEmail(idOrEmail), this.logger);
            updatedInternalPatient.Id = idOrEmail;
            return await ExceptionHandler.ExecuteAndHandleAsync(async () =>
                await this.patientDao.PatchPatient(updatedInternalPatient, oldPatient), this.logger);
        }
    }
}