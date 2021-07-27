using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using QMUL.DiabetesBackend.DataInterfaces;
using QMUL.DiabetesBackend.Model;
using QMUL.DiabetesBackend.Model.Enums;
using QMUL.DiabetesBackend.ServiceInterfaces;

namespace QMUL.DiabetesBackend.ServiceImpl.Implementations
{
    public class AlexaService : IAlexaService
    {
        private readonly IPatientDao patientDao;
        private readonly IMedicationRequestDao medicationRequestDao;
        private readonly IServiceRequestDao serviceRequestDao;
        private readonly ICarePlanDao carePlanDao;
        private readonly IEventDao eventDao;

        public AlexaService(IPatientDao patientDao, IMedicationRequestDao medicationRequestDao,
            IServiceRequestDao serviceRequestDao, ICarePlanDao carePlanDao, IEventDao eventDao)
        {
            this.patientDao = patientDao;
            this.medicationRequestDao = medicationRequestDao;
            this.serviceRequestDao = serviceRequestDao;
            this.carePlanDao = carePlanDao;
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
                AlexaRequestTime.ExactTime => await eventDao.GetEvents(patient.Id, type, dateTime, timing),
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
                    timing),
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

        private static Bundle GenerateEmptyBundle()
        {
            return new()
            {
                Type = Bundle.BundleType.Searchset,
                Timestamp = DateTimeOffset.UtcNow
            };
        }

        private async Task<List<MedicationRequest>> GetMedicationBundle(IEnumerable<HealthEvent> events)
        {
            var uniqueIds = new HashSet<string>();
            uniqueIds.UnionWith(events.Select(item => item.Resource.ResourceId).ToArray());
            return await medicationRequestDao.GetMedicationRequestsByIds(uniqueIds.ToArray());
        }

        private async Task<List<ServiceRequest>> GetServiceBundle(IEnumerable<HealthEvent> events)
        {
            var uniqueIds = new HashSet<string>();
            uniqueIds.UnionWith(events.Select(item => item.Resource.ResourceId).ToArray());
            return await serviceRequestDao.GetServiceRequestsByIds(uniqueIds.ToArray());
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
            var result = await this.patientDao.UpdatePatient(patient);
            if (!result)
            {
                throw new ArgumentException("Could not update the patient's event.", nameof(eventTiming));
            }

            return await this.eventDao.UpdateEvents(patient.Id, eventTiming, dateTime);
        }
    }
}
