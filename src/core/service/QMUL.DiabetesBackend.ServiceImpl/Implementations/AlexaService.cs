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
        private const int DefaultTimingOffset = 20; // The offset for related timings. E.g., before lunch and lunch

        public AlexaService(IPatientDao patientDao, IMedicationRequestDao medicationRequestDao,
            IServiceRequestDao serviceRequestDao, IEventDao eventDao)
        {
            this.patientDao = patientDao;
            this.medicationRequestDao = medicationRequestDao;
            this.serviceRequestDao = serviceRequestDao;
            this.eventDao = eventDao;
        }

        public async Task<Bundle> ProcessRequest(string patientEmailOrId, AlexaRequestType type, DateTime dateTime,
            CustomEventTiming timing, string timezone = "UTC")
        {
            switch (type)
            {
                case AlexaRequestType.Medication:
                    return await GetMedicationRequests(patientEmailOrId, dateTime, timing, false, timezone);
                case AlexaRequestType.Insulin:
                    return await GetMedicationRequests(patientEmailOrId, dateTime, timing, true, timezone);
                case AlexaRequestType.Glucose:
                    return await GetServiceRequests(patientEmailOrId, dateTime, timing, timezone);
                case AlexaRequestType.Appointment:
                    break;
                case AlexaRequestType.CarePlan:
                    return await GetCarePlan(patientEmailOrId, dateTime, timing, timezone);
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

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
            patient.ExactEventTimes = SetRelatedTimings(patient, eventTiming, dateTime);
            await this.UpdatePatient(patient);

            return await this.UpdateRelatedTimingEvents(patient, eventTiming, dateTime);
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

        /// <summary>
        /// Sets exact times for related timings i.e., breakfast, lunch and dinner timings. If there are no related timings,
        /// it will set just the given timing.
        /// </summary>
        /// <param name="patient">The patient</param>
        /// <param name="timing">The timing to compare</param>
        /// <param name="dateTime">The exact time of the event</param>
        /// <returns>The updated event times for the patient.</returns>
        private static Dictionary<CustomEventTiming, DateTime> SetRelatedTimings(Patient patient,
            CustomEventTiming timing, DateTime dateTime)
        {
            dateTime = AdjustOffsetTiming(timing, dateTime);
            var before = dateTime.AddMinutes(DefaultTimingOffset * -1);
            var after = dateTime.AddMinutes(DefaultTimingOffset);
            patient.ExactEventTimes ??= new Dictionary<CustomEventTiming, DateTime>();
            switch (timing)
            {
                case CustomEventTiming.CM:
                case CustomEventTiming.ACM:
                case CustomEventTiming.PCM:
                    patient.ExactEventTimes[CustomEventTiming.CM] = dateTime;
                    patient.ExactEventTimes[CustomEventTiming.ACM] = before;
                    patient.ExactEventTimes[CustomEventTiming.PCM] = after;
                    break;
                case CustomEventTiming.CD:
                case CustomEventTiming.ACD:
                case CustomEventTiming.PCD:
                    patient.ExactEventTimes[CustomEventTiming.CD] = dateTime;
                    patient.ExactEventTimes[CustomEventTiming.ACD] = before;
                    patient.ExactEventTimes[CustomEventTiming.PCD] = after;
                    break;
                case CustomEventTiming.CV:
                case CustomEventTiming.ACV:
                case CustomEventTiming.PCV:
                    patient.ExactEventTimes[CustomEventTiming.CV] = dateTime;
                    patient.ExactEventTimes[CustomEventTiming.ACV] = before;
                    patient.ExactEventTimes[CustomEventTiming.PCV] = after;
                    break;
                default:
                    patient.ExactEventTimes[timing] = dateTime;
                    break;
            }

            return patient.ExactEventTimes;
        }

        private static DateTime AdjustOffsetTiming(CustomEventTiming timing, DateTime dateTime)
        {
            return timing switch
            {
                CustomEventTiming.ACM => dateTime.AddMinutes(DefaultTimingOffset),
                CustomEventTiming.ACD => dateTime.AddMinutes(DefaultTimingOffset),
                CustomEventTiming.ACV => dateTime.AddMinutes(DefaultTimingOffset),
                CustomEventTiming.PCM => dateTime.AddMinutes(DefaultTimingOffset * -1),
                CustomEventTiming.PCD => dateTime.AddMinutes(DefaultTimingOffset * -1),
                CustomEventTiming.PCV => dateTime.AddMinutes(DefaultTimingOffset * -1),
                _ => dateTime
            };
        }
        
        private async Task<Bundle> GetMedicationRequests(string patientEmailOrId, DateTime dateTime,
            CustomEventTiming timing, bool insulin, string timezone = "UTC")
        {
            var patient = await patientDao.GetPatientByIdOrEmail(patientEmailOrId);
            var type = insulin ? EventType.InsulinDosage : EventType.MedicationDosage;
            var timings = EventTimingMapper.GetRelatedTimings(timing);
            IEnumerable<HealthEvent> events;
            if (timings.Length == 0)
            {
                var (start, end) = EventTimingMapper.GetIntervalForPatient(patient, dateTime, timing, timezone, DefaultOffset);
                events = await eventDao.GetEvents(patient.Id, type, start, end);
            }
            else
            {
                var (start, end) = EventTimingMapper.GetRelativeDayInterval(dateTime, timezone);
                events = await eventDao.GetEvents(patient.Id, type, start, end, timings);
            }

            var bundle = ResourceUtils.GenerateEmptyBundle();
            var medicationRequests = await GetMedicationBundle(events);

            bundle.Entry = medicationRequests.Select(request => new Bundle.EntryComponent {Resource = request})
                .ToList();
            return bundle;
        }

        private async Task<Bundle> GetServiceRequests(string patientEmailOrId, DateTime dateTime,
            CustomEventTiming timing, string timezone = "UTC")
        {
            var patient = await patientDao.GetPatientByIdOrEmail(patientEmailOrId);
            var timings = EventTimingMapper.GetRelatedTimings(timing);
            IEnumerable<HealthEvent> events;
            if (timings.Length == 0)
            {
                var (start, end) = EventTimingMapper.GetIntervalForPatient(patient, dateTime, timing, timezone, DefaultOffset);
                events = await eventDao.GetEvents(patient.Id, EventType.Measurement, start, end);
            }
            else
            {
                var (start, end) = EventTimingMapper.GetRelativeDayInterval(dateTime, timezone);
                events = await eventDao.GetEvents(patient.Id, EventType.Measurement, start, end, timings);
            }

            var bundle = ResourceUtils.GenerateEmptyBundle();
            var serviceRequests = await GetServiceBundle(events);
            bundle.Entry = serviceRequests.Select(request => new Bundle.EntryComponent {Resource = request})
                .ToList();
            return bundle;
        }
        
        private async Task<Bundle> GetCarePlan(string patientEmailOrId, DateTime dateTime,
            CustomEventTiming timing, string timezone = "UTC")
        {
            var patient = await patientDao.GetPatientByIdOrEmail(patientEmailOrId);
            var timings = EventTimingMapper.GetRelatedTimings(timing);
            var types = new[] {EventType.Measurement, EventType.InsulinDosage, EventType.MedicationDosage} ;
            IEnumerable<HealthEvent> events;
            if (timings.Length == 0)
            {
                var (start, end) = EventTimingMapper.GetIntervalForPatient(patient, dateTime, timing, timezone, DefaultOffset);
                events = await eventDao.GetEvents(patient.Id, types, start, end);
            }
            else
            {
                var (start, end) = EventTimingMapper.GetRelativeDayInterval(dateTime, timezone);
                events = await eventDao.GetEvents(patient.Id, types, start, end, timings);
            }

            var bundle = ResourceUtils.GenerateEmptyBundle();
            var healthEvents = events.ToList();
            var serviceEvents = healthEvents.Where(healthEvent => healthEvent.Resource.EventType == EventType.Measurement).ToArray();
            var serviceRequests = await GetServiceBundle(serviceEvents);
            var serviceEntries = serviceRequests.Select(request => new Bundle.EntryComponent {Resource = request})
                .ToList();
            var medicationEvents = healthEvents.Where(healthEvent =>
                healthEvent.Resource.EventType is EventType.MedicationDosage or EventType.InsulinDosage).ToArray();
            var medicationRequests = await GetMedicationBundle(medicationEvents);
            var medicationEntries = medicationRequests.Select(request => new Bundle.EntryComponent {Resource = request})
                .ToList();
            serviceEntries.AddRange(medicationEntries);
            bundle.Entry = serviceEntries;
            return bundle;
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

        /// <summary>
        /// Updates all events related to a timing event, i.e., breakfast, lunch, and dinner.
        /// </summary>
        private async Task<bool> UpdateRelatedTimingEvents(Patient patient, CustomEventTiming timing, DateTime dateTime)
        {
            bool result;
            switch (timing)
            {
                case CustomEventTiming.CM:
                case CustomEventTiming.ACM:
                case CustomEventTiming.PCM:
                    result = await this.eventDao.UpdateEventsTiming(patient.Id, CustomEventTiming.CM,
                        patient.ExactEventTimes[CustomEventTiming.CM]);
                    result = result && await this.eventDao.UpdateEventsTiming(patient.Id, CustomEventTiming.ACM,
                        patient.ExactEventTimes[CustomEventTiming.ACM]);
                    result = result && await this.eventDao.UpdateEventsTiming(patient.Id, CustomEventTiming.PCM,
                        patient.ExactEventTimes[CustomEventTiming.PCM]);
                    return result;
                case CustomEventTiming.CD:
                case CustomEventTiming.ACD:
                case CustomEventTiming.PCD:
                    result = await this.eventDao.UpdateEventsTiming(patient.Id, CustomEventTiming.CD,
                        patient.ExactEventTimes[CustomEventTiming.CD]);
                    result = result && await this.eventDao.UpdateEventsTiming(patient.Id, CustomEventTiming.ACD,
                        patient.ExactEventTimes[CustomEventTiming.ACD]);
                    result = result && await this.eventDao.UpdateEventsTiming(patient.Id, CustomEventTiming.PCD,
                        patient.ExactEventTimes[CustomEventTiming.PCD]);
                    return result;
                case CustomEventTiming.CV:
                case CustomEventTiming.ACV:
                case CustomEventTiming.PCV:
                    result = await this.eventDao.UpdateEventsTiming(patient.Id, CustomEventTiming.CV,
                        patient.ExactEventTimes[CustomEventTiming.CV]);
                    result = result && await this.eventDao.UpdateEventsTiming(patient.Id, CustomEventTiming.ACV,
                        patient.ExactEventTimes[CustomEventTiming.ACV]);
                    result = result && await this.eventDao.UpdateEventsTiming(patient.Id, CustomEventTiming.PCV,
                        patient.ExactEventTimes[CustomEventTiming.PCV]);
                    return result;
                default:
                    return await this.eventDao.UpdateEventsTiming(patient.Id, timing, dateTime);
            }
        }
    }
}
