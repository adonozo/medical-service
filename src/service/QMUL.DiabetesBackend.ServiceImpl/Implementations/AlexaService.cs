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
                    return await this.GetMedicationRequests(patientEmailOrId, dateTime, requestTime, timing);
                case AlexaRequestType.Insulin:
                    break;
                case AlexaRequestType.Glucose:
                    break;
                case AlexaRequestType.Appointment:
                    break;
                case AlexaRequestType.CarePlan:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
            
            throw new NotImplementedException();
        }

        public async Task<Bundle> GetMedicationRequests(string patientEmailOrId, DateTime dateTime, AlexaRequestTime requestTime,
            CustomEventTiming timing)
        {
            var patient = await this.patientDao.GetPatientByIdOrEmail(patientEmailOrId);
            IEnumerable<HealthEvent> events;
            switch (requestTime)
            {
                case AlexaRequestTime.ExactTime:
                    events = await this.eventDao.GetEvents(patient.Id.ToString(), EventType.MedicationDosage, dateTime, timing);
                    break;
                case AlexaRequestTime.AllDay:
                    throw new NotImplementedException();
                case AlexaRequestTime.OnEvent:
                    events = await this.eventDao.GetEvents(patient.Id.ToString(), EventType.MedicationDosage, dateTime, timing);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(requestTime), requestTime, null);
            }

            var bundle = GenerateEmptyBundle();
            var medicationRequests = await this.GetMedicationBundle(events);
            bundle.Entry = medicationRequests.Select(request => new Bundle.EntryComponent{ Resource = request}).ToList();
            return bundle;
        }

        public Task<Bundle> GetServiceRequests(string patientEmailOrId, DateTime dateTime, AlexaRequestTime requestTime,
            CustomEventTiming timing)
        {
            throw new NotImplementedException();
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
    }
}
