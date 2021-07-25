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

        public async Task<Bundle> ProcessRequest(string patientEmailOrId, AlexaRequestType type, DateTime dateTime, AlexaRequestTime requestTime,
            CustomEventTiming timing)
        {
            switch (type)
            {
                case AlexaRequestType.Medication:
                    return await this.GetMedicationRequests(patientEmailOrId, dateTime, requestTime, timing, false);
                case AlexaRequestType.Insulin:
                    return await this.GetMedicationRequests(patientEmailOrId, dateTime, requestTime, timing, true);
                case AlexaRequestType.Glucose:
                    return await this.GetMeasurements(patientEmailOrId, dateTime, requestTime, timing);
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
            var patient = await this.patientDao.GetPatientByIdOrEmail(patientEmailOrId);
            var type = insulin ? EventType.InsulinDosage : EventType.MedicationDosage;
            var events = requestTime switch
            {
                AlexaRequestTime.ExactTime => await this.eventDao.GetEvents(patient.Id.ToString(),
                    type, dateTime, timing),
                AlexaRequestTime.AllDay => await this.eventDao.GetEvents(patient.Id.ToString(),
                    type, dateTime),
                AlexaRequestTime.OnEvent => await this.eventDao.GetEvents(patient.Id.ToString(),
                    type, dateTime, timing),
                _ => throw new ArgumentOutOfRangeException(nameof(requestTime), requestTime, null)
            };

            var bundle = GenerateEmptyBundle();
            var medicationRequests = await this.GetMedicationBundle(events);
            bundle.Entry = medicationRequests.Select(request => new Bundle.EntryComponent {Resource = request})
                .ToList();
            return bundle;
        }

        public async Task<Bundle> GetServiceRequests(string patientEmailOrId, DateTime dateTime, AlexaRequestTime requestTime,
            CustomEventTiming timing)
        {
            var patient = await this.patientDao.GetPatientByIdOrEmail(patientEmailOrId);
            var events = requestTime switch
            {
                AlexaRequestTime.ExactTime => await this.eventDao.GetEvents(patient.Id.ToString(),
                    EventType.Measurement, dateTime, timing),
                AlexaRequestTime.AllDay => await this.eventDao.GetEvents(patient.Id.ToString(),
                    EventType.Measurement, dateTime),
                AlexaRequestTime.OnEvent => await this.eventDao.GetEvents(patient.Id.ToString(),
                    EventType.Measurement, dateTime, timing),
                _ => throw new ArgumentOutOfRangeException(nameof(requestTime), requestTime, null)
            };

            var bundle = GenerateEmptyBundle();
            var serviceRequests = await this.GetServiceBundle(events);
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
            return await this.medicationRequestDao.GetMedicationRequestsByIds(uniqueIds.ToArray());
        }
        
        private async Task<List<ServiceRequest>> GetServiceBundle(IEnumerable<HealthEvent> events)
        {
            var uniqueIds = new HashSet<string>();
            uniqueIds.UnionWith(events.Select(item => item.Resource.ResourceId).ToArray());
            return await this.serviceRequestDao.GetServiceRequestsByIds(uniqueIds.ToArray());
        }
    }
}
