namespace QMUL.DiabetesBackend.Service;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataInterfaces;
using Hl7.Fhir.Model;
using Microsoft.Extensions.Logging;
using Model;
using Model.Enums;
using Model.Exceptions;
using Model.Extensions;
using ServiceInterfaces;
using Utils;
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
    public async Task<Bundle?> ProcessMedicationRequest(string patientEmailOrId,
        DateTime dateTime,
        CustomEventTiming timing,
        string timezone = "UTC")
    {
        logger.LogTrace("Processing Alexa Medication request type");
        return await this.GetMedicationRequests(
            patientEmailOrId: patientEmailOrId,
            dateTime: dateTime,
            timing: timing,
            type: EventType.MedicationDosage,
            timezone: timezone);
    }

    /// <inheritdoc/>
    public async Task<Bundle?> ProcessInsulinMedicationRequest(string patientEmailOrId,
        DateTime dateTime,
        CustomEventTiming timing,
        string timezone = "UTC")
    {
        logger.LogTrace("Processing Alexa Insulin request type");
        return await this.GetMedicationRequests(
            patientEmailOrId: patientEmailOrId,
            dateTime: dateTime,
            timing: timing,
            type: EventType.InsulinDosage,
            timezone: timezone);
    }

    /// <inheritdoc/>
    public async Task<Bundle?> ProcessGlucoseServiceRequest(string patientEmailOrId, DateTime dateTime,
        CustomEventTiming timing, string timezone = "UTC")
    {
        logger.LogTrace("Processing Alexa Glucose service request type");
        var patient = await this.patientDao.GetPatientByIdOrEmail(patientEmailOrId);
        if (patient is null)
        {
            return null;
        }

        var timingPreferences = patient.GetTimingPreference();
        var timings = EventTimingMapper.GetRelatedTimings(timing);
        IEnumerable<HealthEvent> events;
        if (timings.Length == 0)
        {
            var (start, end) = EventTimingMapper.GetIntervalForPatient(
                preferences: timingPreferences,
                startTime: dateTime,
                timing: timing,
                timezone: timezone,
                defaultOffset: DefaultOffset);
            events = await this.eventDao.GetEvents(
                patientId: patient.Id,
                type: EventType.Measurement,
                start: start.UtcDateTime,
                end: end.UtcDateTime);
        }
        else
        {
            var (start, end) = EventTimingMapper.GetRelativeDayInterval(dateTime, timezone);
            events = await this.eventDao.GetEvents(
                patientId: patient.Id,
                type: EventType.Measurement,
                start: start.UtcDateTime,
                end: end.UtcDateTime,
                timings: timings);
        }

        var bundle = await this.GenerateBundle(events.ToList());
        return bundle;
    }

    /// <inheritdoc/>
    public async Task<Bundle?> ProcessCarePlanRequest(string patientEmailOrId, DateTime dateTime,
        CustomEventTiming timing, string timezone = "UTC")
    {
        this.logger.LogTrace("Processing Alexa Care Plan request type");
        var patient = await this.patientDao.GetPatientByIdOrEmail(patientEmailOrId);
        if (patient is null)
        {
            return null;
        }

        var timingPreferences = patient.GetTimingPreference();
        var timings = EventTimingMapper.GetRelatedTimings(timing);
        var types = new[] { EventType.Measurement, EventType.InsulinDosage, EventType.MedicationDosage };
        IEnumerable<HealthEvent> events;
        if (timings.Length == 0)
        {
            var (start, end) = EventTimingMapper.GetIntervalForPatient(
                preferences: timingPreferences,
                startTime: dateTime,
                timing: timing,
                timezone: timezone,
                defaultOffset: DefaultOffset);
            events = await this.eventDao.GetEvents(
                patientId: patient.Id,
                types: types,
                start: start.UtcDateTime,
                end: end.UtcDateTime);
        }
        else
        {
            var (start, end) = EventTimingMapper.GetRelativeDayInterval(dateTime, timezone);
            events = await this.eventDao.GetEvents(
                patientId: patient.Id,
                types: types,
                start: start.UtcDateTime,
                end: end.UtcDateTime,
                timings: timings);
        }

        var bundle = await this.GenerateBundle(events.ToList());
        return bundle;
    }

    /// <inheritdoc/>
    public async Task<Bundle?> GetNextRequests(string patientEmailOrId, AlexaRequestType type)
    {
        var patient = await this.patientDao.GetPatientByIdOrEmail(patientEmailOrId);
        if (patient is null)
        {
            return null;
        }

        if (type is not (AlexaRequestType.Glucose or AlexaRequestType.Insulin or AlexaRequestType.Medication))
        {
            return ResourceUtils.GenerateSearchBundle(Array.Empty<Resource>());
        }

        var evenType = ResourceUtils.MapRequestToEventType(type);
        var events = await this.eventDao.GetNextEvents(patient.Id, evenType);
        var bundle = await this.GenerateBundle(events.ToList());
        return bundle;
    }

    /// <inheritdoc/>
    public async Task<Bundle?> GetNextRequests(string patientEmailOrId)
    {
        var patient = await this.patientDao.GetPatientByIdOrEmail(patientEmailOrId);
        if (patient is null)
        {
            return null;
        }

        var types = new[] { EventType.Measurement, EventType.InsulinDosage, EventType.MedicationDosage };
        var events = await this.eventDao.GetNextEvents(patient.Id, types);
        var bundle = await this.GenerateBundle(events.ToList());
        return bundle;
    }

    /// <inheritdoc/>
    public async Task<bool> UpsertTimingEvent(string patientIdOrEmail, CustomEventTiming eventTiming,
        DateTime dateTime)
    {
        var patient = await ResourceUtils.GetResourceOrThrowAsync(
            () => this.patientDao.GetPatientByIdOrEmail(patientIdOrEmail),
            new ValidationException("PatientNotFound"));

        var timingPreferences = patient.GetTimingPreference();
        timingPreferences = SetRelatedTimings(timingPreferences, eventTiming, dateTime);
        patient.SetTimingPreferences(timingPreferences);

        await this.patientDao.UpdatePatient(patient);
        this.logger.LogDebug("Timing event updated for {IdOrEmail}: {Timing}, {DateTime}", patientIdOrEmail,
            eventTiming, dateTime);
        return await this.UpdateRelatedTimingEvents(patient.Id, timingPreferences, eventTiming, dateTime);
    }

    /// <inheritdoc/>
    public async Task<bool> UpsertDosageStartDate(string patientIdOrEmail, string dosageId, DateTime startDate)
    {
        var patient = await ResourceUtils.GetResourceOrThrowAsync(
            () => this.patientDao.GetPatientByIdOrEmail(patientIdOrEmail),
            new ValidationException("PatientNotFound"));

        var internalPatient = patient.ToInternalPatient();
        await this.SetDosageStartDate(internalPatient, dosageId, startDate);
        this.logger.LogDebug("Dosage start date updated for {IdOrEmail}: {DosageId}, {DateTime}", patientIdOrEmail,
            dosageId, startDate);
        return true;
    }

    /// <inheritdoc/>
    public async Task<bool> UpsertServiceRequestStartDate(string patientIdOrEmail, string serviceRequestId,
        DateTime startDate)
    {
        var patient = await ResourceUtils.GetResourceOrThrowAsync(
            () => this.patientDao.GetPatientByIdOrEmail(patientIdOrEmail),
            new ValidationException("PatientNotFound"));
        var internalPatient = patient.ToInternalPatient();

        await this.SetServiceRequestStartDate(internalPatient, serviceRequestId, startDate);
        return true;
    }

    /// <summary>
    /// Sets exact times for related timings i.e., breakfast, lunch and dinner timings. If there are no related timings,
    /// it will set just the given timing.
    /// </summary>
    /// <param name="preferences">A dictionary with the the timing as keys and the datetime as value</param>
    /// <param name="timing">The timing to compare</param>
    /// <param name="dateTime">The exact time of the event</param>
    /// <returns>The updated event times for the patient.</returns>
    private static Dictionary<CustomEventTiming, DateTimeOffset> SetRelatedTimings(
        Dictionary<CustomEventTiming, DateTimeOffset> preferences,
        CustomEventTiming timing, DateTime dateTime)
    {
        dateTime = AdjustOffsetTiming(timing, dateTime);
        var before = dateTime.AddMinutes(DefaultTimingOffset * -1);
        var after = dateTime.AddMinutes(DefaultTimingOffset);
        switch (timing)
        {
            case CustomEventTiming.CM:
            case CustomEventTiming.ACM:
            case CustomEventTiming.PCM:
                preferences[CustomEventTiming.CM] = dateTime;
                preferences[CustomEventTiming.ACM] = before;
                preferences[CustomEventTiming.PCM] = after;
                break;
            case CustomEventTiming.CD:
            case CustomEventTiming.ACD:
            case CustomEventTiming.PCD:
                preferences[CustomEventTiming.CD] = dateTime;
                preferences[CustomEventTiming.ACD] = before;
                preferences[CustomEventTiming.PCD] = after;
                break;
            case CustomEventTiming.CV:
            case CustomEventTiming.ACV:
            case CustomEventTiming.PCV:
                preferences[CustomEventTiming.CV] = dateTime;
                preferences[CustomEventTiming.ACV] = before;
                preferences[CustomEventTiming.PCV] = after;
                break;
            default:
                preferences[timing] = dateTime;
                break;
        }

        return preferences;
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

    private async Task<Bundle?> GetMedicationRequests(string patientEmailOrId,
        DateTime dateTime,
        CustomEventTiming timing,
        EventType type,
        string timezone = "UTC")
    {
        var patient = await this.patientDao.GetPatientByIdOrEmail(patientEmailOrId);
        if (patient is null)
        {
            return null;
        }

        var timingPreferences = patient.GetTimingPreference();
        var timings = EventTimingMapper.GetRelatedTimings(timing);
        IEnumerable<HealthEvent> events;
        if (timings.Length == 0)
        {
            var (start, end) = EventTimingMapper.GetIntervalForPatient(
                preferences: timingPreferences,
                startTime: dateTime,
                timing: timing,
                timezone: timezone,
                defaultOffset: DefaultOffset);
            events = await this.eventDao.GetEvents(
                patientId: patient.Id,
                type: type,
                start: start.UtcDateTime,
                end: end.UtcDateTime);
        }
        else
        {
            var (start, end) = EventTimingMapper.GetRelativeDayInterval(dateTime, timezone);
            events = await this.eventDao.GetEvents(
                patientId: patient.Id,
                type: type,
                start: start.UtcDateTime,
                end: end.UtcDateTime,
                timings: timings);
        }

        return await this.GenerateBundle(events.ToList());
    }

    private async Task<Bundle> GenerateBundle(IReadOnlyCollection<HealthEvent> healthEvents)
    {
        var serviceEvents = healthEvents
            .Where(healthEvent => healthEvent.ResourceReference.EventType == EventType.Measurement).ToArray();
        var serviceRequests = serviceEvents.Any()
            ? await this.GetServiceBundle(serviceEvents)
            : new List<ServiceRequest>();
        var medicationEvents = healthEvents.Where(healthEvent =>
                healthEvent.ResourceReference.EventType is EventType.MedicationDosage or EventType.InsulinDosage)
            .ToArray();
        var medicationRequests = medicationEvents.Any()
            ? await this.GetMedicationBundle(medicationEvents)
            : new List<MedicationRequest>();

        var entries = new List<Resource>(serviceRequests);
        entries.AddRange(medicationRequests);
        return ResourceUtils.GenerateSearchBundle(entries);
    }

    private async Task<IList<MedicationRequest>> GetMedicationBundle(IEnumerable<HealthEvent> events)
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

    private async Task<IList<ServiceRequest>> GetServiceBundle(IEnumerable<HealthEvent> events)
    {
        var uniqueIds = new HashSet<string>();
        uniqueIds.UnionWith(events.Select(item => item.ResourceReference.ResourceId).ToArray());
        return await this.serviceRequestDao.GetServiceRequestsByIds(uniqueIds.ToArray());
    }

    /// <summary>
    /// Updates all events related to a timing event, i.e., breakfast, lunch, and dinner.
    /// </summary>
    private async Task<bool> UpdateRelatedTimingEvents(string patientId,
        IReadOnlyDictionary<CustomEventTiming, DateTimeOffset> preferences, CustomEventTiming timing,
        DateTimeOffset dateTime)
    {
        bool result;
        switch (timing)
        {
            case CustomEventTiming.CM:
            case CustomEventTiming.ACM:
            case CustomEventTiming.PCM:
                result = await this.eventDao.UpdateEventsTiming(patientId, CustomEventTiming.CM,
                    preferences[CustomEventTiming.CM]);
                result = result && await this.eventDao.UpdateEventsTiming(patientId, CustomEventTiming.ACM,
                    preferences[CustomEventTiming.ACM]);
                result = result && await this.eventDao.UpdateEventsTiming(patientId, CustomEventTiming.PCM,
                    preferences[CustomEventTiming.PCM]);
                return result;
            case CustomEventTiming.CD:
            case CustomEventTiming.ACD:
            case CustomEventTiming.PCD:
                result = await this.eventDao.UpdateEventsTiming(patientId, CustomEventTiming.CD,
                    preferences[CustomEventTiming.CD]);
                result = result && await this.eventDao.UpdateEventsTiming(patientId, CustomEventTiming.ACD,
                    preferences[CustomEventTiming.ACD]);
                result = result && await this.eventDao.UpdateEventsTiming(patientId, CustomEventTiming.PCD,
                    preferences[CustomEventTiming.PCD]);
                return result;
            case CustomEventTiming.CV:
            case CustomEventTiming.ACV:
            case CustomEventTiming.PCV:
                result = await this.eventDao.UpdateEventsTiming(patientId, CustomEventTiming.CV,
                    preferences[CustomEventTiming.CV]);
                result = result && await this.eventDao.UpdateEventsTiming(patientId, CustomEventTiming.ACV,
                    preferences[CustomEventTiming.ACV]);
                result = result && await this.eventDao.UpdateEventsTiming(patientId, CustomEventTiming.PCV,
                    preferences[CustomEventTiming.PCV]);
                return result;
            default:
                return await this.eventDao.UpdateEventsTiming(patientId, timing, dateTime);
        }
    }

    /// <summary>
    /// Sets the start date for a medication's request dosage.
    /// Updates a list of health events that belongs to a medication request that has a specific dosage ID. To update
    /// events, they are deleted and created again. 
    /// </summary>
    /// <param name="patient">The <see cref="InternalPatient"/> related to the medication request</param>
    /// <param name="dosageId">The dosage ID to update. A medication request will be fetched using this value.</param>
    /// <param name="startDate">When this dosage has been started.</param>
    /// <returns>True if the update was successful. False otherwise.</returns>
    /// <exception cref="ArgumentException">If the events were not deleted.</exception>
    private async Task SetDosageStartDate(InternalPatient patient, string dosageId, DateTime startDate)
    {
        var medicationRequest = await this.medicationRequestDao.GetMedicationRequestForDosage(patient.Id, dosageId);
        if (medicationRequest is null)
        {
            throw new NotFoundException();
        }

        var index = medicationRequest.DosageInstruction.FindIndex(dose => dose.ElementId == dosageId);
        if (index < 0)
        {
            throw new ValidationException($"Could not get the dosage from the medication request {dosageId}");
        }

        medicationRequest.DosageInstruction[index].SetStartDate(startDate);
        await this.medicationRequestDao.UpdateMedicationRequest(medicationRequest.Id, medicationRequest);

        medicationRequest = GetMedicationRequestWithSingleDosage(medicationRequest, dosageId);
        await this.UpdateHealthEvents(medicationRequest, patient);
    }

    private static MedicationRequest GetMedicationRequestWithSingleDosage(MedicationRequest request,
        string dosageId)
    {
        var dosage = request.DosageInstruction.FirstOrDefault(dose => dose.ElementId == dosageId);
        if (dosage == null)
        {
            throw new ValidationException($"Could not get the dosage from the medication request {dosageId}");
        }

        request.DosageInstruction = new List<Dosage> { dosage };
        return request;
    }

    private async Task SetServiceRequestStartDate(InternalPatient patient, string serviceRequestId,
        DateTime startDate)
    {
        var serviceRequest = await this.serviceRequestDao.GetServiceRequest(serviceRequestId);
        if (serviceRequest is null)
        {
            throw new NotFoundException();
        }

        serviceRequest.SetStartDate(startDate);
        await this.serviceRequestDao.UpdateServiceRequest(serviceRequestId, serviceRequest);
        await this.UpdateHealthEvents(serviceRequest, patient);
    }

    private async Task UpdateHealthEvents(DomainResource request, InternalPatient patient)
    {
        var deleteEvents = await this.eventDao.DeleteEventSeries(request.Id);
        if (!deleteEvents)
        {
            this.logger.LogWarning("Could not delete events series for dosage {Id}", request.Id);
            throw new WriteResourceException("Unable to delete events for requested dosage");
        }

        var events = ResourceUtils.GenerateEventsFrom(request, patient);
        await this.eventDao.CreateEvents(events);
    }
}