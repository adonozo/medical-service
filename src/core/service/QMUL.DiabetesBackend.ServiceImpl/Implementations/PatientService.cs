namespace QMUL.DiabetesBackend.ServiceImpl.Implementations
{
    using System.Threading.Tasks;
    using DataInterfaces;
    using Hl7.Fhir.Model;
    using ServiceInterfaces;
    using Microsoft.Extensions.Logging;
    using Model;
    using Model.Exceptions;
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
        public async Task<PaginatedResult<Bundle>> GetPatientList(PaginationRequest paginationRequest)
        {
            var paginatedPatients = await this.patientDao.GetPatients(paginationRequest);
            return paginatedPatients.ToBundleResult();
        }

        /// <inheritdoc/>
        public Task<Patient> CreatePatient(Patient newInternalPatient)
        {
            this.logger.LogDebug("Creating new patient");
            return this.patientDao.CreatePatient(newInternalPatient);
        }

        /// <inheritdoc/>
        public Task<Patient?> GetPatient(string idOrEmail)
        {
            return this.patientDao.GetPatientByIdOrEmail(idOrEmail);
        }

        /// <inheritdoc/>
        public async Task<Patient> UpdatePatient(string idOrEmail, Patient updatedInternalPatient)
        {
            var patientNotFoundException = new NotFoundException($"Patient not found: {idOrEmail}");
            await ResourceUtils.GetResourceOrThrow(async () =>
                await this.patientDao.GetPatientByIdOrEmail(idOrEmail), patientNotFoundException);

            updatedInternalPatient.Id = idOrEmail;
            return await this.patientDao.UpdatePatient(updatedInternalPatient);
        }

        /// <inheritdoc/>
        public async Task<Patient> PatchPatient(string idOrEmail, InternalPatient updatedInternalPatient)
        {
            var patientNotFoundException = new NotFoundException($"Patient not found: {idOrEmail}");
            var oldPatient = await ResourceUtils.GetResourceOrThrow(async () =>
                await this.patientDao.GetPatientByIdOrEmail(idOrEmail), patientNotFoundException);

            updatedInternalPatient.Id = idOrEmail;
            return await this.patientDao.PatchPatient(updatedInternalPatient, oldPatient);
        }
    }
}
