namespace QMUL.DiabetesBackend.ServiceImpl.Implementations
{
    using System;
    using System.Threading.Tasks;
    using DataInterfaces;
    using Hl7.Fhir.Model;
    using Microsoft.Extensions.Logging;
    using Model.Extensions;
    using ServiceInterfaces;
    using Utils;
    using Task = System.Threading.Tasks.Task;

    /// <summary>
    /// Manages Medication Requests
    /// </summary>
    public class MedicationRequestService : IMedicationRequestService
    {
        private readonly IMedicationRequestDao medicationRequestDao;
        private readonly IMedicationDao medicationDao;
        private readonly IEventDao eventDao;
        private readonly IPatientDao patientDao;
        private readonly ILogger<MedicationRequestService> logger;

        public MedicationRequestService(IMedicationRequestDao medicationRequestDao, IEventDao eventDao,
            IPatientDao patientDao, IMedicationDao medicationDao, ILogger<MedicationRequestService> logger)
        {
            this.medicationRequestDao = medicationRequestDao;
            this.eventDao = eventDao;
            this.patientDao = patientDao;
            this.medicationDao = medicationDao;
            this.logger = logger;
        }

        /// <inheritdoc/>>
        public async Task<MedicationRequest> CreateMedicationRequest(MedicationRequest request)
        {
            var patient = await ExceptionHandler.ExecuteAndHandleAsync(async () =>
                await this.patientDao.GetPatientByIdOrEmail(request.Subject.GetPatientIdFromReference()), this.logger);
            this.logger.LogDebug("Creating medication request for patient {PatientId}", patient.Id);
            var internalPatient = patient.ToInternalPatient();

            await this.SetInsulinRequest(request);
            request.AuthoredOn = DateTime.UtcNow.ToString("O");
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
            await this.SetInsulinRequest(request);
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
            await this.eventDao.DeleteRelatedEvents(id);
            return await this.medicationRequestDao.DeleteMedicationRequest(id);
        }

        /// <inheritdoc/>>
        public async Task<Bundle> GetActiveMedicationRequests(string patientIdOrEmail)
        {
            var patient = await ExceptionHandler.ExecuteAndHandleAsync(async () =>
                await this.patientDao.GetPatientByIdOrEmail(patientIdOrEmail), this.logger);
            var medicationRequests = await this.medicationRequestDao.GetActiveMedicationRequests(patient.Id);
            this.logger.LogDebug("Found {Count} active medication requests", medicationRequests.Count);
            return ResourceUtils.GenerateSearchBundle(medicationRequests);
        }

        /// <summary>
        /// Checks if the <see cref="MedicationRequest"/> has am insulin type <see cref="Medication"/>. If this is true,
        /// it adds the insulin flag extension to the medication request.
        /// The medication can be contained as part of the request, or be just a reference with the medication ID.
        /// </summary>
        /// <param name="request">The <see cref="MedicationRequest"/>.</param>
        public async Task SetInsulinRequest(MedicationRequest request)
        {
            var medicationReference = request.Medication;
            if (medicationReference is not ResourceReference reference || string.IsNullOrWhiteSpace(reference.Reference))
            {
                return;
            }

            if (request.FindContainedResource(reference.Reference) is not Medication medication)
            {
                var medicationId = reference.GetPatientIdFromReference();
                medication = await this.medicationDao.GetSingleMedication(medicationId);
            }

            if (medication != null && medication.HasInsulinFlag())
            {
                request.SetInsulinFlag();
            }
        }
    }
}