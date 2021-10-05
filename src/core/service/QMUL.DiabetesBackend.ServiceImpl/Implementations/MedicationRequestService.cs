namespace QMUL.DiabetesBackend.ServiceImpl.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using DataInterfaces;
    using Hl7.Fhir.Model;
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

        public MedicationRequestService(IMedicationRequestDao medicationRequestDao, IEventDao eventDao, IPatientDao patientDao)
        {
            this.medicationRequestDao = medicationRequestDao;
            this.eventDao = eventDao;
            this.patientDao = patientDao;
        }

        /// <inheritdoc/>>
        public async Task<MedicationRequest> CreateMedicationRequest(MedicationRequest request)
        {
            var patient = await ResourceUtils.ValidateObject(
                () => this.patientDao.GetPatientByIdOrEmail(request.Subject.ElementId),
                "Unable to find patient for the Observation", new KeyNotFoundException());
            var newRequest = await this.medicationRequestDao.CreateMedicationRequest(request);
            var events = EventsGenerator.GenerateEventsFrom(newRequest, patient);
            var eventsResult = await this.eventDao.CreateEvents(events);
            if (!eventsResult)
            {
                throw new ArgumentException($"Unable to create events related to the request: {newRequest.Id}");
            }

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

            return result;
        }

        /// <inheritdoc/>>
        public Task<MedicationRequest> UpdateMedicationRequest(string id, MedicationRequest request)
        {
            var exists = this.medicationRequestDao.GetMedicationRequest(id) != null;
            if (exists)
            {
                return this.medicationRequestDao.UpdateMedicationRequest(id, request);
            }

            throw new KeyNotFoundException();
        }

        /// <inheritdoc/>>
        public Task<bool> DeleteMedicationRequest(string id)
        {
            var exists = this.medicationRequestDao.GetMedicationRequest(id) != null;
            if (exists)
            {
                return this.medicationRequestDao.DeleteMedicationRequest(id);
            }

            throw new KeyNotFoundException();
        }

        /// <inheritdoc/>>
        public async Task<Bundle> GetActiveMedicationRequests(string patientIdOrEmail)
        {
            var patient = await this.patientDao.GetPatientByIdOrEmail(patientIdOrEmail);
            if (patient == null)
            {
                throw new KeyNotFoundException();
            }
            
            var bundle = ResourceUtils.GenerateEmptyBundle();
            var medicationRequests = await this.medicationRequestDao.GetActiveMedicationRequests(patient.Id);
            bundle.Entry = medicationRequests.Select(request => new Bundle.EntryComponent {Resource = request})
                .ToList();
            return bundle;
        }
    }
}
