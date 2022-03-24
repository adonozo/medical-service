namespace QMUL.DiabetesBackend.ServiceImpl.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using DataInterfaces;
    using Hl7.Fhir.Model;
    using Microsoft.Extensions.Logging;
    using Model;
    using Model.Enums;
    using ServiceInterfaces;
    using ServiceInterfaces.Exceptions;
    using Utils;
    using NotFoundException = DataInterfaces.Exceptions.NotFoundException;
    using Task = System.Threading.Tasks.Task;

    /// <summary>
    /// The Alexa Service handles requests for Alexa, getting the Bundle of medication and service requests, and creating
    /// or updating patient settings (e.g., timing events).
    /// </summary>
    public class AlexaService : IAlexaService
    {
        private readonly IPatientDao patientDao;
        private readonly IMedicationRequestDao medicationRequestDao;
        private readonly IServiceRequestDao serviceRequestDao;
        private readonly IEventDao eventDao;
        private readonly ILogger<AlexaService> logger;
        private const int DefaultOffset = 20; // The default offset (in minutes) when looking for exact times
        private const int DefaultTimingOffset = 20; // The offset for related timings. E.g., before lunch and lunch

        public AlexaService(IPatientDao patientDao, IMedicationRequestDao medicationRequestDao,
            IServiceRequestDao serviceRequestDao, IEventDao eventDao, ILogger<AlexaService> logger)
        {
            this.patientDao = patientDao;
            this.medicationRequestDao = medicationRequestDao;
            this.serviceRequestDao = serviceRequestDao;
            this.eventDao = eventDao;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public async Task<Bundle> ProcessMedicationRequest(string patientEmailOrId, DateTime dateTime,
            CustomEventTiming timing, string timezone = "UTC")
        {
            logger.LogTrace("Processing Alexa Medication request type");
            return await this.GetMedicationRequests(patientEmailOrId, dateTime, timing, EventType.MedicationDosage,
                timezone);
        }

        /// <inheritdoc/>
        public async Task<Bundle> ProcessInsulinMedicationRequest(string patientEmailOrId, DateTime dateTime,
            CustomEventTiming timing, string timezone = "UTC")
        {
            logger.LogTrace("Processing Alexa Insulin request type");
            return await this.GetMedicationRequests(patientEmailOrId, dateTime, timing, EventType.InsulinDosage,
                timezone);
        }

        /// <inheritdoc/>
        public async Task<Bundle> ProcessGlucoseServiceRequest(string patientEmailOrId, DateTime dateTime,
            CustomEventTiming timing, string timezone = "UTC")
        {
            logger.LogTrace("Processing Alexa Glucose service request type");
            var patient = await ExceptionHandler.ExecuteAndHandleAsync(async () =>
                await this.patientDao.GetPatientByIdOrEmail(patientEmailOrId), this.logger);
            var internalPatient = patient.ToInternalPatient();
            var timings = EventTimingMapper.GetRelatedTimings(timing);
            IEnumerable<HealthEvent> events;
            if (timings.Length == 0)
            {
                var (start, end) =
                    EventTimingMapper.GetIntervalForPatient(internalPatient, dateTime, timing, timezone, DefaultOffset);
                events = await this.eventDao.GetEvents(patient.Id, EventType.Measurement, start.UtcDateTime, end.UtcDateTime);
            }
            else
            {
                var (start, end) = EventTimingMapper.GetRelativeDayInterval(dateTime, timezone);
                events = await this.eventDao.GetEvents(patient.Id, EventType.Measurement, start.UtcDateTime, end.UtcDateTime, timings);
            }

            var bundle = await this.GenerateBundle(events.ToList());
            return bundle;
        }

        /// <inheritdoc/>
        public async Task<Bundle> ProcessCarePlanRequest(string patientEmailOrId, DateTime dateTime,
            CustomEventTiming timing, string timezone = "UTC")
        {
            this.logger.LogTrace("Processing Alexa Care Plan request type");
            var patient = await ExceptionHandler.ExecuteAndHandleAsync(async () =>
                await this.patientDao.GetPatientByIdOrEmail(patientEmailOrId), this.logger);
            var internalPatient = patient.ToInternalPatient();
            var timings = EventTimingMapper.GetRelatedTimings(timing);
            var types = new[] {EventType.Measurement, EventType.InsulinDosage, EventType.MedicationDosage};
            IEnumerable<HealthEvent> events;
            if (timings.Length == 0)
            {
                var (start, end) =
                    EventTimingMapper.GetIntervalForPatient(internalPatient, dateTime, timing, timezone, DefaultOffset);
                events = await this.eventDao.GetEvents(patient.Id, types, start.UtcDateTime, end.UtcDateTime);
            }
            else
            {
                var (start, end) = EventTimingMapper.GetRelativeDayInterval(dateTime, timezone);
                events = await this.eventDao.GetEvents(patient.Id, types, start.UtcDateTime, end.UtcDateTime, timings);
            }

            var bundle = await this.GenerateBundle(events.ToList());
            return bundle;
        }

        /// <inheritdoc/>
        public async Task<Bundle> GetNextRequests(string patientEmailOrId, AlexaRequestType type)
        {
            var patient = await ExceptionHandler.ExecuteAndHandleAsync(async () =>
                await this.patientDao.GetPatientByIdOrEmail(patientEmailOrId), this.logger);
            if (type is not (AlexaRequestType.Glucose or AlexaRequestType.Insulin or AlexaRequestType.Medication))
            {
                this.logger.LogDebug("Unsupported request type: {Type}", type);
                throw new NotSupportedException("Request type not supported");
            }

            var evenType = ResourceUtils.MapRequestToEventType(type);
            var events = await this.eventDao.GetNextEvents(patient.Id, evenType);
            var bundle = await this.GenerateBundle(events.ToList());
            return bundle;
        }

        /// <inheritdoc/>
        public async Task<Bundle> GetNextRequests(string patientEmailOrId)
        {
            var patient = await ExceptionHandler.ExecuteAndHandleAsync(async () =>
                await this.patientDao.GetPatientByIdOrEmail(patientEmailOrId), this.logger);
            var types = new[] {EventType.Measurement, EventType.InsulinDosage, EventType.MedicationDosage};
            var events = await this.eventDao.GetNextEvents(patient.Id, types);
            var bundle = await this.GenerateBundle(events.ToList());
            return bundle;
        }

        /// <inheritdoc/>
        public async Task<bool> UpsertTimingEvent(string patientIdOrEmail, CustomEventTiming eventTiming,
            DateTime dateTime)
        {
            var patient = await ExceptionHandler.ExecuteAndHandleAsync(async () =>
                await this.patientDao.GetPatientByIdOrEmail(patientIdOrEmail), this.logger);
            // ToDo maybe this conversion is unnecessary
            var internalPatient = patient.ToInternalPatient();
            var timingPreferences = SetRelatedTimings(internalPatient, eventTiming, dateTime);
            patient.SetTimingPreferences(timingPreferences);
            await ExceptionHandler.ExecuteAndHandleAsync(async () =>
                await this.patientDao.UpdatePatient(patient), this.logger);
            this.logger.LogDebug("Timing event updated for {IdOrEmail}: {Timing}, {DateTime}", patientIdOrEmail,
                eventTiming, dateTime);
            return await this.UpdateRelatedTimingEvents(internalPatient, eventTiming, dateTime);
        }

        /// <inheritdoc/>
        public async Task<bool> UpsertDosageStartDate(string patientIdOrEmail, string dosageId, DateTime startDate)
        {
            var patient = await ExceptionHandler.ExecuteAndHandleAsync(async () =>
                await this.patientDao.GetPatientByIdOrEmail(patientIdOrEmail), this.logger);
            var internalPatient = patient.ToInternalPatient();
            internalPatient.ResourceStartDate[dosageId] = startDate;
            await ExceptionHandler.ExecuteAndHandleAsync(async () =>
                await this.patientDao.UpdatePatient(patient), this.logger);
            await this.UpdateHealthEvents(internalPatient, dosageId);
            this.logger.LogDebug("Dosage start date updated for {IdOrEmail}: {DosageId}, {DateTime}", patientIdOrEmail,
                dosageId, startDate);
            return true;
        }

        /// <summary>
        /// Sets exact times for related timings i.e., breakfast, lunch and dinner timings. If there are no related timings,
        /// it will set just the given timing.
        /// </summary>
        /// <param name="patient">The patient</param>
        /// <param name="timing">The timing to compare</param>
        /// <param name="dateTime">The exact time of the event</param>
        /// <returns>The updated event times for the patient.</returns>
        private static Dictionary<CustomEventTiming, DateTimeOffset> SetRelatedTimings(InternalPatient patient,
            CustomEventTiming timing, DateTime dateTime)
        {
            dateTime = AdjustOffsetTiming(timing, dateTime);
            var before = dateTime.AddMinutes(DefaultTimingOffset * -1);
            var after = dateTime.AddMinutes(DefaultTimingOffset);
            patient.ExactEventTimes ??= new Dictionary<CustomEventTiming, DateTimeOffset>();
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
            CustomEventTiming timing, EventType type, string timezone = "UTC")
        {
            var patient = await ExceptionHandler.ExecuteAndHandleAsync(async () =>
                await this.patientDao.GetPatientByIdOrEmail(patientEmailOrId), this.logger);
            var internalPatient = patient.ToInternalPatient();
            var timings = EventTimingMapper.GetRelatedTimings(timing);
            IEnumerable<HealthEvent> events;
            if (timings.Length == 0)
            {
                var (start, end) =
                    EventTimingMapper.GetIntervalForPatient(internalPatient, dateTime, timing, timezone, DefaultOffset);
                events = await this.eventDao.GetEvents(patient.Id, type, start.UtcDateTime, end.UtcDateTime);
            }
            else
            {
                var (start, end) = EventTimingMapper.GetRelativeDayInterval(dateTime, timezone);
                events = await this.eventDao.GetEvents(patient.Id, type, start.UtcDateTime, end.UtcDateTime, timings);
            }

            var bundle = await this.GenerateBundle(events.ToList());
            return bundle;
        }

        private async Task<Bundle> GenerateBundle(IReadOnlyCollection<HealthEvent> healthEvents)
        {
            var bundle = ResourceUtils.GenerateEmptyBundle();
            var serviceEvents = healthEvents
                .Where(healthEvent => healthEvent.ResourceReference.EventType == EventType.Measurement).ToArray();
            var serviceRequests = serviceEvents.Any()
                ? await this.GetServiceBundle(serviceEvents)
                : new List<ServiceRequest>();
            var serviceEntries = serviceRequests.Select(request => new Bundle.EntryComponent {Resource = request})
                .ToList();
            var medicationEvents = healthEvents.Where(healthEvent =>
                healthEvent.ResourceReference.EventType is EventType.MedicationDosage or EventType.InsulinDosage).ToArray();
            var medicationRequests = medicationEvents.Any()
                ? await this.GetMedicationBundle(medicationEvents)
                : new List<MedicationRequest>();
            var medicationEntries = medicationRequests
                .Select(request => new Bundle.EntryComponent {Resource = request})
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
                uniqueRequestIds.Add(item.ResourceReference.ResourceId);
                uniqueDosageIds.Add(item.ResourceReference.EventReferenceId);
            }

            var requests = await this.medicationRequestDao.GetMedicationRequestsByIds(uniqueRequestIds.ToArray());
            foreach (var request in requests)
            {
                // Remove all dosages that are not in the event. This is necessary when the medication request has
                // dosages that may not be related with these events; e.g., dosages in different days.
                request.DosageInstruction.RemoveAll(dose => !uniqueDosageIds.Contains(dose.ElementId));
            }

            return requests;
        }

        private async Task<List<ServiceRequest>> GetServiceBundle(IEnumerable<HealthEvent> events)
        {
            var uniqueIds = new HashSet<string>();
            uniqueIds.UnionWith(events.Select(item => item.ResourceReference.ResourceId).ToArray());
            return await this.serviceRequestDao.GetServiceRequestsByIds(uniqueIds.ToArray());
        }

        /// <summary>
        /// Updates all events related to a timing event, i.e., breakfast, lunch, and dinner.
        /// </summary>
        private async Task<bool> UpdateRelatedTimingEvents(InternalPatient patient, CustomEventTiming timing, DateTimeOffset dateTime)
        {
            return await ExceptionHandler.ExecuteAndHandleAsync(async () =>
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
            }, this.logger);
        }

        /// <summary>
        /// Updates a list of health events that belongs to a medication request that has a specific dosage ID. To update
        /// events, they are deleted and created again. 
        /// </summary>
        /// <param name="patient">The <see cref="InternalPatient"/> related to the medication request</param>
        /// <param name="dosageId">The dosage ID to update. A medication request will be fetched using this value.</param>
        /// <returns>True if the update was successful. False otherwise.</returns>
        /// <exception cref="ArgumentException">If the events were not deleted.</exception>
        private async Task UpdateHealthEvents(InternalPatient patient, string dosageId)
        {
            var medicationRequest = await ExceptionHandler.ExecuteAndHandleAsync(async () =>
                await this.medicationRequestDao.GetMedicationRequestForDosage(patient.Id, dosageId), this.logger);
            var deleteEvents = await this.eventDao.DeleteEventSeries(dosageId);
            if (!deleteEvents)
            {
                this.logger.LogWarning("Could not delete events series for dosage {Id}", dosageId);
                throw new UpdateException("Unable to delete events for requested dosage");
            }

            medicationRequest = GetMedicationRequestWithSingleDosage(medicationRequest, dosageId);
            var events = ResourceUtils.GenerateEventsFrom(medicationRequest, patient);
            await ExceptionHandler.ExecuteAndHandleAsync(async () =>
                await this.eventDao.CreateEvents(events), this.logger);
        }

        private static MedicationRequest GetMedicationRequestWithSingleDosage(MedicationRequest request,
            string dosageId)
        {
            var dosage = request.DosageInstruction.FirstOrDefault(dose => dose.ElementId == dosageId);
            if (dosage == null)
            {
                throw new NotFoundException("Could not get the dosage from the medication request");
            }

            request.DosageInstruction = new List<Dosage> {dosage};
            return request;
        }
    }
}