using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using QMUL.DiabetesBackend.DataInterfaces;
using QMUL.DiabetesBackend.Model;
using QMUL.DiabetesBackend.Model.Enums;
using QMUL.DiabetesBackend.ServiceImpl.Utils;
using QMUL.DiabetesBackend.ServiceInterfaces;
using Patient = QMUL.DiabetesBackend.Model.Patient;

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
            var newRequest = await this.medicationRequestDao.CreateMedicationRequest(request);
            var patient = await this.patientDao.GetPatientByIdOrEmail(request.Subject.ElementId);
            var events = this.GenerateEventsFrom(newRequest, patient);
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

        /// <summary>
        /// Creates a list of events based on medication timings.
        /// </summary>
        /// <param name="request">The medication request</param>
        /// <param name="patient">The medication request's subject</param>
        /// <returns>A List of events for the medication request</returns>
        private IEnumerable<HealthEvent> GenerateEventsFrom(MedicationRequest request, Patient patient)
        {
            var events = new List<HealthEvent>();
            var requestReference = new CustomResource
            {
                EventType = EventType.MedicationDosage
            };

            foreach (var dosage in request.DosageInstruction)
            {
                requestReference.ResourceId = request.Id;
                requestReference.Text = dosage.Text;
                requestReference.EventReferenceId = dosage.ElementId;
                var eventsGenerator = new EventsGenerator(patient, dosage.Timing, requestReference);
                events.AddRange(eventsGenerator.GetEvents());
            }
            
            return events;
        }
    }
}
