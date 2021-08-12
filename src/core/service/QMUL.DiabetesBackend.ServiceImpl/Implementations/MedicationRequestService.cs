using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using QMUL.DiabetesBackend.DataInterfaces;
using QMUL.DiabetesBackend.ServiceImpl.Utils;
using QMUL.DiabetesBackend.ServiceInterfaces;

namespace QMUL.DiabetesBackend.ServiceImpl.Implementations
{
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

        public async Task<MedicationRequest> GetMedicationRequest(string id)
        {
            var result = await this.medicationRequestDao.GetMedicationRequest(id);
            if (result == null)
            {
                throw new KeyNotFoundException();
            }

            return result;
        }

        public Task<MedicationRequest> UpdateMedicationRequest(string id, MedicationRequest request)
        {
            var exists = this.medicationRequestDao.GetMedicationRequest(id) != null;
            if (exists)
            {
                return this.medicationRequestDao.UpdateMedicationRequest(id, request);
            }

            throw new KeyNotFoundException();
        }

        public Task<bool> DeleteMedicationRequest(string id)
        {
            var exists = this.medicationRequestDao.GetMedicationRequest(id) != null;
            if (exists)
            {
                return this.medicationRequestDao.DeleteMedicationRequest(id);
            }

            throw new KeyNotFoundException();
        }
    }
}
