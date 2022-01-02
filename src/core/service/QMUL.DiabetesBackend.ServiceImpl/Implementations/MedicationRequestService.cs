namespace QMUL.DiabetesBackend.ServiceImpl.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using DataInterfaces;
    using Hl7.Fhir.Model;
    using Microsoft.Extensions.Logging;
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
            var patient = await ResourceUtils.ValidateNullObject(
                () => this.patientDao.GetPatientByIdOrEmail(request.Subject.ElementId),
                new KeyNotFoundException("Unable to find the patient linked to the medication request"));
            this.logger.LogDebug("Creating medication request for patient {PatientId}", patient.Id);
            var newRequest = await this.medicationRequestDao.CreateMedicationRequest(request);
            var events = ResourceUtils.GenerateEventsFrom(newRequest, patient);
            await ResourceUtils.ValidateBooleanResult(() => this.eventDao.CreateEvents(events),
                new ArgumentException($"Unable to create events related to the request: {newRequest.Id}"));
            this.logger.LogDebug("Medication request created with ID {Id}", newRequest.Id);
            return newRequest;
        }

        /// <inheritdoc/>>
        public async Task<MedicationRequest> GetMedicationRequest(string id)
        {
            var medicationRequest = await ResourceUtils.ValidateNullObject(
                () => this.medicationRequestDao.GetMedicationRequest(id),
                new KeyNotFoundException($"Unable to find the medication request with ID {id}"));

            this.logger.LogDebug("Found medication request with ID {Id}", id);
            return medicationRequest;
        }

        /// <inheritdoc/>>
        public async Task<MedicationRequest> UpdateMedicationRequest(string id, MedicationRequest request)
        {
            await ResourceUtils.ValidateNullObject(
                () => this.medicationRequestDao.GetMedicationRequest(id),
                new KeyNotFoundException($"The medication request with ID {id} was not found"));

            this.logger.LogDebug("Medication request with ID {Id} updated", id);
            return await this.medicationRequestDao.UpdateMedicationRequest(id, request);
        }

        /// <inheritdoc/>>
        public async Task<bool> DeleteMedicationRequest(string id)
        {
            await ResourceUtils.ValidateNullObject(
                () => this.medicationRequestDao.GetMedicationRequest(id),
                new KeyNotFoundException($"The medication request with ID {id} was not found"));

            this.logger.LogDebug("Medication request deleted {Id}", id);
            return await this.medicationRequestDao.DeleteMedicationRequest(id);
        }

        /// <inheritdoc/>>
        public async Task<Bundle> GetActiveMedicationRequests(string patientIdOrEmail)
        {
            var patient = await ResourceUtils.ValidateNullObject(
                () => this.patientDao.GetPatientByIdOrEmail(patientIdOrEmail),
                new KeyNotFoundException($"Unable to find the patient with ID {patientIdOrEmail}"));

            var bundle = ResourceUtils.GenerateEmptyBundle();
            var medicationRequests = await this.medicationRequestDao.GetActiveMedicationRequests(patient.Id);
            bundle.Entry = medicationRequests.Select(request => new Bundle.EntryComponent {Resource = request})
                .ToList();
            this.logger.LogDebug("Found {Count} active medication requests", medicationRequests.Count);
            return bundle;
        }
    }
}
