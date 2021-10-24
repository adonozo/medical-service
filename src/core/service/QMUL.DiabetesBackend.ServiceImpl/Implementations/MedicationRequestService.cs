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
            var patient = await ResourceUtils.ValidateObject(
                () => this.patientDao.GetPatientByIdOrEmail(request.Subject.ElementId),
                "Unable to find patient for the Observation", new KeyNotFoundException());
            this.logger.LogDebug("Creating medication request for patient {PatientId}", patient.Id);
            var newRequest = await this.medicationRequestDao.CreateMedicationRequest(request);
            var events = EventsGenerator.GenerateEventsFrom(newRequest, patient);
            var eventsResult = await this.eventDao.CreateEvents(events);
            if (!eventsResult)
            {
                throw new ArgumentException($"Unable to create events related to the request: {newRequest.Id}");
            }

            this.logger.LogDebug("Medication request created with ID {Id}", newRequest.Id);
            return newRequest;
        }

        /// <inheritdoc/>>
        public async Task<MedicationRequest> GetMedicationRequest(string id)
        {
            var result = await this.medicationRequestDao.GetMedicationRequest(id);
            if (result == null)
            {
                throw new KeyNotFoundException();
            }

            this.logger.LogDebug("Found medication request with ID {Id}", id);
            return result;
        }

        /// <inheritdoc/>>
        public Task<MedicationRequest> UpdateMedicationRequest(string id, MedicationRequest request)
        {
            var exists = this.medicationRequestDao.GetMedicationRequest(id) != null;
            if (!exists)
            {
                throw new KeyNotFoundException();
            }

            this.logger.LogDebug("Medication request with ID {Id} updated", id);
            return this.medicationRequestDao.UpdateMedicationRequest(id, request);
        }

        /// <inheritdoc/>>
        public Task<bool> DeleteMedicationRequest(string id)
        {
            var exists = this.medicationRequestDao.GetMedicationRequest(id) != null;
            if (!exists)
            {
                throw new KeyNotFoundException();
            }

            this.logger.LogDebug("Medication request deleted {Id}", id);
            return this.medicationRequestDao.DeleteMedicationRequest(id);
        }

        /// <inheritdoc/>>
        public async Task<Bundle> GetActiveMedicationRequests(string patientIdOrEmail)
        {
            var patient = await this.patientDao.GetPatientByIdOrEmail(patientIdOrEmail);
            if (patient == null)
            {
                this.logger.LogWarning("Patient not found: {PatientIdOrEmail}", patientIdOrEmail);
                throw new KeyNotFoundException();
            }
            
            var bundle = ResourceUtils.GenerateEmptyBundle();
            var medicationRequests = await this.medicationRequestDao.GetActiveMedicationRequests(patient.Id);
            bundle.Entry = medicationRequests.Select(request => new Bundle.EntryComponent {Resource = request})
                .ToList();
            this.logger.LogDebug("Found {Count} active medication requests", medicationRequests.Count);
            return bundle;
        }
    }
}
