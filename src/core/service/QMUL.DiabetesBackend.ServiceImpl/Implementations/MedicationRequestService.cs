namespace QMUL.DiabetesBackend.ServiceImpl.Implementations
{
    using System.Linq;
    using System.Threading.Tasks;
    using DataInterfaces;
    using Hl7.Fhir.Model;
    using Microsoft.Extensions.Logging;
    using Model;
    using ServiceInterfaces;
    using Utils;

    /// <summary>
    /// Manages Medication Requests
    /// </summary>
    public class MedicationRequestService : IMedicationRequestService
    {
        private readonly IMedicationRequestDao medicationRequestDao;
        private readonly IEventDao eventDao;
        private readonly IPatientDao patientDao;
        private readonly ILogger<MedicationRequestService> logger;

        public MedicationRequestService(IMedicationRequestDao medicationRequestDao, IEventDao eventDao,
            IPatientDao patientDao, ILogger<MedicationRequestService> logger)
        {
            this.medicationRequestDao = medicationRequestDao;
            this.eventDao = eventDao;
            this.patientDao = patientDao;
            this.logger = logger;
        }

        /// <inheritdoc/>>
        public async Task<MedicationRequest> CreateMedicationRequest(MedicationRequest request)
        {
            var patient = await ExceptionHandler.ExecuteAndHandleAsync(async () =>
                await this.patientDao.GetPatientByIdOrEmail(request.Subject.ElementId), this.logger);
            this.logger.LogDebug("Creating medication request for patient {PatientId}", patient.Id);
            var internalPatient = patient.ToInternalPatient();
            var newRequest = await ExceptionHandler.ExecuteAndHandleAsync(async () =>
                await this.medicationRequestDao.CreateMedicationRequest(request), this.logger);
            var events = ResourceUtils.GenerateEventsFrom(newRequest, internalPatient);
            await ExceptionHandler.ExecuteAndHandleAsync(async () => await this.eventDao.CreateEvents(events),
                this.logger);
            this.logger.LogDebug("Medication request created with ID {Id}", newRequest.Id);
            return newRequest;
        }

        /// <inheritdoc/>>
        public async Task<MedicationRequest> GetMedicationRequest(string id)
        {
            var medicationRequest = await ExceptionHandler.ExecuteAndHandleAsync(async () =>
                await this.medicationRequestDao.GetMedicationRequest(id), this.logger);
            this.logger.LogDebug("Found medication request with ID {Id}", id);
            return medicationRequest;
        }

        /// <inheritdoc/>>
        public async Task<MedicationRequest> UpdateMedicationRequest(string id, MedicationRequest request)
        {
            // Check if the medication request exists
            await ExceptionHandler.ExecuteAndHandleAsync(async () =>
                await this.medicationRequestDao.GetMedicationRequest(id), this.logger);
            var updatedResult = await ExceptionHandler.ExecuteAndHandleAsync(async () =>
                await this.medicationRequestDao.UpdateMedicationRequest(id, request), this.logger);
            this.logger.LogDebug("Medication request with ID {Id} updated", id);
            return updatedResult;
        }

        /// <inheritdoc/>>
        public async Task<bool> DeleteMedicationRequest(string id)
        {
            // Check if the medication request exists
            await ExceptionHandler.ExecuteAndHandleAsync(async () =>
                await this.medicationRequestDao.GetMedicationRequest(id), this.logger);
            this.logger.LogDebug("Medication request deleted {Id}", id);
            return await this.medicationRequestDao.DeleteMedicationRequest(id);
        }

        /// <inheritdoc/>>
        public async Task<Bundle> GetActiveMedicationRequests(string patientIdOrEmail)
        {
            var patient = await ExceptionHandler.ExecuteAndHandleAsync(async () =>
                await this.patientDao.GetPatientByIdOrEmail(patientIdOrEmail), this.logger);
            var bundle = ResourceUtils.GenerateEmptyBundle();
            var medicationRequests = await this.medicationRequestDao.GetActiveMedicationRequests(patient.Id);
            bundle.Entry = medicationRequests.Select(request => new Bundle.EntryComponent {Resource = request})
                .ToList();
            this.logger.LogDebug("Found {Count} active medication requests", medicationRequests.Count);
            return bundle;
        }
    }
}