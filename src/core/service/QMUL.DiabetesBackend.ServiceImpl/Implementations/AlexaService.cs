using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using QMUL.DiabetesBackend.DataInterfaces;
using QMUL.DiabetesBackend.Model;
using QMUL.DiabetesBackend.Model.Enums;
using QMUL.DiabetesBackend.ServiceImpl.Utils;
using QMUL.DiabetesBackend.ServiceInterfaces;
using Patient = QMUL.DiabetesBackend.Model.Patient;
using Task = System.Threading.Tasks.Task;

namespace QMUL.DiabetesBackend.ServiceImpl.Implementations
{
    public class AlexaService : IAlexaService
    {
        private readonly IPatientDao patientDao;
        private readonly IMedicationRequestDao medicationRequestDao;
        private readonly IServiceRequestDao serviceRequestDao;
        private readonly IEventDao eventDao;
        private const int DefaultOffset = 20; // The default offset (in minutes) when looking for exact times 

        public AlexaService(IPatientDao patientDao, IMedicationRequestDao medicationRequestDao,
            IServiceRequestDao serviceRequestDao, IEventDao eventDao)
        {
            this.patientDao = patientDao;
            this.medicationRequestDao = medicationRequestDao;
            this.serviceRequestDao = serviceRequestDao;
            this.eventDao = eventDao;
        }

        public async Task<Bundle> ProcessRequest(string patientEmailOrId, AlexaRequestType type, DateTime dateTime,
            AlexaRequestTime requestTime,
            CustomEventTiming timing)
        {
            switch (type)
            {
                case AlexaRequestType.Medication:
                    return await GetMedicationRequests(patientEmailOrId, dateTime, requestTime, timing, false);
                case AlexaRequestType.Insulin:
                    return await GetMedicationRequests(patientEmailOrId, dateTime, requestTime, timing, true);
                case AlexaRequestType.Glucose:
                    return await GetMeasurements(patientEmailOrId, dateTime, requestTime, timing);
                case AlexaRequestType.Appointment:
                    break;
                case AlexaRequestType.CarePlan:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            throw new NotImplementedException();
        }

        public async Task<Bundle> GetMedicationRequests(string patientEmailOrId, DateTime dateTime,
            AlexaRequestTime requestTime, CustomEventTiming timing, bool insulin)
        {
            var patient = await patientDao.GetPatientByIdOrEmail(patientEmailOrId);
            var type = insulin ? EventType.InsulinDosage : EventType.MedicationDosage;
            var events = requestTime switch
            {
                AlexaRequestTime.ExactTime => await eventDao.GetEvents(patient.Id, type, dateTime, DefaultOffset),
                AlexaRequestTime.AllDay => await eventDao.GetEvents(patient.Id, type, dateTime),
                AlexaRequestTime.OnEvent => await eventDao.GetEvents(patient.Id, type, dateTime, timing),
                _ => throw new ArgumentOutOfRangeException(nameof(requestTime), requestTime, null)
            };

            var bundle = GenerateEmptyBundle();
            var medicationRequests = await GetMedicationBundle(events);

            bundle.Entry = medicationRequests.Select(request => new Bundle.EntryComponent {Resource = request})
                .ToList();
            return bundle;
        }

        public async Task<Bundle> GetServiceRequests(string patientEmailOrId, DateTime dateTime,
            AlexaRequestTime requestTime, CustomEventTiming timing)
        {
            var patient = await patientDao.GetPatientByIdOrEmail(patientEmailOrId);
            var events = requestTime switch
            {
                AlexaRequestTime.ExactTime => await eventDao.GetEvents(patient.Id, EventType.Measurement, dateTime,
                    DefaultOffset),
                AlexaRequestTime.AllDay => await eventDao.GetEvents(patient.Id, EventType.Measurement, dateTime),
                AlexaRequestTime.OnEvent => await eventDao.GetEvents(patient.Id, EventType.Measurement, dateTime,
                    timing),
                _ => throw new ArgumentOutOfRangeException(nameof(requestTime), requestTime, null)
            };

            var bundle = GenerateEmptyBundle();
            var serviceRequests = await GetServiceBundle(events);
            bundle.Entry = serviceRequests.Select(request => new Bundle.EntryComponent {Resource = request})
                .ToList();
            return bundle;
        }

        public Task<Bundle> GetMeasurements(string patientEmailOrId, DateTime dateTime, AlexaRequestTime requestTime,
            CustomEventTiming timing)
        {
            throw new NotImplementedException();
        }

        public DiagnosticReport SaveGlucoseMeasure(string patientId, DiagnosticReport report)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> UpsertTimingEvent(string patientIdOrEmail, CustomEventTiming eventTiming, DateTime dateTime)
        {
            var patient = await this.patientDao.GetPatientByIdOrEmail(patientIdOrEmail);
            if (patient == null)
            {
                throw new KeyNotFoundException();
            }

            patient.ExactEventTimes ??= new Dictionary<CustomEventTiming, DateTime>();
            patient.ExactEventTimes[eventTiming] = dateTime;
            await this.UpdatePatient(patient);

            return await this.eventDao.UpdateEventsTiming(patient.Id, eventTiming, dateTime);
        }

        public async Task<bool> UpsertDosageStartDate(string patientIdOrEmail, string dosageId, DateTime startDate)
        {
            var patient = await this.patientDao.GetPatientByIdOrEmail(patientIdOrEmail);
            var medicationRequest =
                await this.medicationRequestDao.GetMedicationRequestForDosage(patient.Id, dosageId);
            if (medicationRequest == null)
            {
                throw new KeyNotFoundException();
            }

            patient.ResourceStartDate ??= new Dictionary<string, DateTime>();
            patient.ResourceStartDate[dosageId] = startDate.Date;
            await this.UpdatePatient(patient);

            var deleteEvents = await this.eventDao.DeleteEventSeries(dosageId);
            if (!deleteEvents)
            {
                throw new ArgumentException("Unable to delete events for requested dosage", nameof(dosageId));
            }

            medicationRequest = GetMedicationRequestWithSingleDosage(medicationRequest, dosageId);
            var events = EventsGenerator.GenerateEventsFrom(medicationRequest, patient);
            var eventsResult = await this.eventDao.CreateEvents(events);
            if (!eventsResult)
            {
                throw new ArgumentException($"Unable to create events related to the dosage instruction: {dosageId}");
            }

            return true;
        }
        
        private static Bundle GenerateEmptyBundle()
        {
            return new()
            {
                Type = Bundle.BundleType.Searchset,
                Timestamp = DateTimeOffset.UtcNow
            };
        }
        
        private static MedicationRequest GetMedicationRequestWithSingleDosage(MedicationRequest request, string dosageId)
        {
            var dosage = request.DosageInstruction.FirstOrDefault(dose => dose.ElementId == dosageId);
            if (dosage == null)
            {
                throw new ArgumentException("Could not get the dosage from the medication request", nameof(dosageId));
            }

            request.DosageInstruction = new List<Dosage> {dosage};
            return request;
        }

        private async Task<List<MedicationRequest>> GetMedicationBundle(IEnumerable<HealthEvent> events)
        {
            var uniqueRequestIds = new HashSet<string>();
            var uniqueDosageIds = new HashSet<string>();
            foreach (var item in events)
            {
                uniqueRequestIds.Add(item.Resource.ResourceId);
                uniqueDosageIds.Add(item.Resource.EventReferenceId);
            }
            
            var requests = await this.medicationRequestDao.GetMedicationRequestsByIds(uniqueRequestIds.ToArray());
            foreach (var request in requests)
            {
                // Remove all dosages that are not in the event
                request.DosageInstruction.RemoveAll(dose => !uniqueDosageIds.Contains(dose.ElementId));
            }

            return requests;
        }

        private async Task<List<ServiceRequest>> GetServiceBundle(IEnumerable<HealthEvent> events)
        {
            var uniqueIds = new HashSet<string>();
            uniqueIds.UnionWith(events.Select(item => item.Resource.ResourceId).ToArray());
            return await this.serviceRequestDao.GetServiceRequestsByIds(uniqueIds.ToArray());
        }

        private async Task UpdatePatient(Patient patient)
        {
            var updatePatientResult = await this.patientDao.UpdatePatient(patient);
            if (!updatePatientResult)
            {
                throw new ArgumentException("Could not update the dosage start date", nameof(patient));
            }
        }
    }
}
