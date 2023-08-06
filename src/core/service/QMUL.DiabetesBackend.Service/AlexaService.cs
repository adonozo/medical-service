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
using NodaTime;
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
    private readonly ILogger<AlexaService> logger;

    private const int DefaultOffsetMinutes = 20;
    private const int OffsetBetweenTimingsMinutes = 20; // For related timings. E.g., before lunch and lunch

    public AlexaService(IPatientDao patientDao,
        IMedicationRequestDao medicationRequestDao,
        IServiceRequestDao serviceRequestDao,
        ILogger<AlexaService> logger)
    {
        this.patientDao = patientDao;
        this.medicationRequestDao = medicationRequestDao;
        this.serviceRequestDao = serviceRequestDao;
        this.logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Bundle> SearchMedicationRequests(string patientEmailOrId,
        LocalDate dateTime,
        bool onlyInsulin,
        CustomEventTiming? timing = CustomEventTiming.ALL_DAY,
        string? timezone = "UTC")
    {
        var patient = await ResourceUtils.GetResourceOrThrowAsync(async () =>
            await this.patientDao.GetPatientByIdOrEmail(patientEmailOrId), new NotFoundException());

        var medicationRequests = await this.medicationRequestDao.GetActiveMedicationRequests(
            patientEmailOrId,
            PaginationRequest.FirstPaginatedResults,
            onlyInsulin);

        if (medicationRequests.Results.Any(request => request.NeedsStartDate()))
        {
            var errorBundle = ResourceUtils.GenerateSearchBundle(medicationRequests.Results);
            return errorBundle;
        }

        var dateFilter = EventTimingMapper.TimingIntervalForPatient(
            patientTimingPreferences: patient.GetTimingPreference(),
            localDate: dateTime,
            timing: timing ?? CustomEventTiming.ALL_DAY,
            timezone: timezone ?? "UTC",
            defaultOffset: DefaultOffsetMinutes);

        var events = this.GetHealthEvents(medicationRequests.Results,
            patient.ToInternalPatient(),
            dateFilter);

        return this.EventsToBundle(events.ToList());
    }

    /// <inheritdoc/>
    public async Task<Bundle?> SearchServiceRequests(string patientEmailOrId,
        LocalDate dateTime,
        CustomEventTiming timing,
        string timezone = "UTC")
    {
        var patient = await ResourceUtils.GetResourceOrThrowAsync(async () =>
            await this.patientDao.GetPatientByIdOrEmail(patientEmailOrId), new NotFoundException());

        var serviceRequests = await this.serviceRequestDao.GetActiveServiceRequests(patientEmailOrId);
        var requestNeedStartDate = serviceRequests.Where(request => request.NeedsStartDate()).ToList();
        if (requestNeedStartDate.Any())
        {
            return ResourceUtils.GenerateSearchBundle(requestNeedStartDate);
        }

        var dateFilter = EventTimingMapper.TimingIntervalForPatient(
            patientTimingPreferences: patient.GetTimingPreference(),
            localDate: dateTime,
            timing: timing,
            timezone: timezone,
            defaultOffset: DefaultOffsetMinutes);

        var events = this.GetHealthEvents(serviceRequests, patient.ToInternalPatient(), dateFilter);
        return this.EventsToBundle(events.ToList());
    }

    /// <inheritdoc/>
    public async Task<bool> UpsertTimingEvent(string patientIdOrEmail, CustomEventTiming eventTiming,
        LocalTime localTime)
    {
        var patient = await ResourceUtils.GetResourceOrThrowAsync(
            () => this.patientDao.GetPatientByIdOrEmail(patientIdOrEmail),
            new ValidationException("PatientNotFound"));

        var timingPreferences = patient.GetTimingPreference();
        timingPreferences = SetRelatedTimings(timingPreferences, eventTiming, localTime);
        patient.SetTimingPreferences(timingPreferences);

        this.logger.LogDebug("Timing event updated for {IdOrEmail}: {Timing}, {DateTime}", patientIdOrEmail,
            eventTiming, localTime);
        return await this.patientDao.UpdatePatient(patient);
    }

    /// <inheritdoc/>
    public async Task<bool> UpsertDosageStartDateTime(string patientIdOrEmail,
        string dosageId,
        LocalDate startDate,
        LocalTime? localTime = null)
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
        LocalDate startDate)
    {
        var serviceRequest = await ResourceUtils.GetResourceOrThrowAsync(
            () => this.serviceRequestDao.GetServiceRequest(serviceRequestId),
            new NotFoundException());

        if (serviceRequest.Occurrence is not Timing timing)
        {
            throw new InvalidOperationException($"Service Request {serviceRequestId} does not have a valid occurrence");
        }

        timing.SetStartDate(startDate);
        timing.RemoveNeedsStartDateFlag();
        serviceRequest.Occurrence = timing;

        return await this.serviceRequestDao.UpdateServiceRequest(serviceRequestId, serviceRequest);
    }

    /// <summary>
    /// Sets exact times for related timings i.e., breakfast, lunch and dinner timings. If there are no related timings,
    /// it will set just the given timing.
    /// </summary>
    /// <param name="preferences">A dictionary with the the timing as keys and the datetime as value</param>
    /// <param name="timing">The timing to compare</param>
    /// <param name="localTime">The exact time of the event</param>
    /// <returns>The updated event times for the patient.</returns>
    private static Dictionary<CustomEventTiming, LocalTime> SetRelatedTimings(
        Dictionary<CustomEventTiming, LocalTime> preferences,
        CustomEventTiming timing,
        LocalTime localTime)
    {
        localTime = AdjustOffsetTiming(timing, localTime);

        switch (timing)
        {
            case CustomEventTiming.CM:
            case CustomEventTiming.ACM:
            case CustomEventTiming.PCM:
                (preferences[CustomEventTiming.CM],
                    preferences[CustomEventTiming.ACM],
                    preferences[CustomEventTiming.PCM]) = GetTimeIntervals(localTime);
                break;
            case CustomEventTiming.CD:
            case CustomEventTiming.ACD:
            case CustomEventTiming.PCD:
                (preferences[CustomEventTiming.CD],
                    preferences[CustomEventTiming.ACD],
                    preferences[CustomEventTiming.PCD]) = GetTimeIntervals(localTime);
                break;
            case CustomEventTiming.CV:
            case CustomEventTiming.ACV:
            case CustomEventTiming.PCV:
                (preferences[CustomEventTiming.CV],
                    preferences[CustomEventTiming.ACV],
                    preferences[CustomEventTiming.PCV]) = GetTimeIntervals(localTime);
                break;
            default:
                preferences[timing] = localTime;
                break;
        }

        return preferences;
    }

    private static LocalTime AdjustOffsetTiming(CustomEventTiming timing, LocalTime localTime)
    {
        return timing switch
        {
            CustomEventTiming.ACM => localTime.PlusMinutes(OffsetBetweenTimingsMinutes),
            CustomEventTiming.ACD => localTime.PlusMinutes(OffsetBetweenTimingsMinutes),
            CustomEventTiming.ACV => localTime.PlusMinutes(OffsetBetweenTimingsMinutes),
            CustomEventTiming.PCM => localTime.PlusMinutes(OffsetBetweenTimingsMinutes * -1),
            CustomEventTiming.PCD => localTime.PlusMinutes(OffsetBetweenTimingsMinutes * -1),
            CustomEventTiming.PCV => localTime.PlusMinutes(OffsetBetweenTimingsMinutes * -1),
            _ => localTime
        };
    }

    private Bundle EventsToBundle(IEnumerable<HealthEvent<MedicationRequest>> healthEvents)
    {
        var medications = new Dictionary<string, MedicationRequest>();
        var dosagesPerRequest = new Dictionary<string, List<Dosage>>(); // Group dosages for the same medication request
        foreach (var @event in healthEvents)
        {
            if (!medications.ContainsKey(@event.Resource.Id))
            {
                medications.Add(@event.Resource.Id, @event.Resource);
                dosagesPerRequest.Add(@event.Resource.Id, new List<Dosage>{ @event.Dosage });
                continue;
            }

            dosagesPerRequest[@event.Resource.Id].Add(@event.Dosage);
        }

        var result = medications.Select(keyValue =>
        {
            var request = keyValue.Value;
            request.DosageInstruction = dosagesPerRequest[keyValue.Key];
            return request;
        });

        return ResourceUtils.GenerateSearchBundle(result);
    }

    private Bundle EventsToBundle(IEnumerable<HealthEvent<ServiceRequest>> healthEvents)
    {
        var requests = healthEvents.Select(@event => @event.Resource);
        return ResourceUtils.GenerateSearchBundle(requests);
    }

    private async Task SetDosageStartDate(InternalPatient patient,
        string dosageId,
        LocalDate startDate,
        LocalTime? startTime = null)
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

        medicationRequest.DosageInstruction[index].Timing.SetStartDate(startDate);
        medicationRequest.DosageInstruction[index].Timing.RemoveNeedsStartDateFlag();

        if (startTime.HasValue)
        {
            medicationRequest.DosageInstruction[index].Timing.SetStartTime(startTime.Value);
            medicationRequest.DosageInstruction[index].Timing.RemoveNeedsStartTimeFlag();
        }

        await this.medicationRequestDao.UpdateMedicationRequest(medicationRequest.Id, medicationRequest);
    }

    private IEnumerable<HealthEvent<MedicationRequest>> GetHealthEvents(
        IEnumerable<MedicationRequest> medicationRequests,
        InternalPatient patient,
        Interval dateFilter)
    {
        return medicationRequests
            .Select(request => ResourceUtils.GenerateEventsFrom(request, patient, dateFilter))
            .SelectMany(result => result);
    }

    private IEnumerable<HealthEvent<ServiceRequest>> GetHealthEvents(
        IEnumerable<ServiceRequest> serviceRequests,
        InternalPatient patient,
        Interval dateFilter)
    {
        return serviceRequests
            .Select(request => ResourceUtils.GenerateEventsFrom(request, patient, dateFilter))
            .SelectMany(result => result);
    }

    private static (LocalTime middle, LocalTime before, LocalTime after) GetTimeIntervals(LocalTime localTime)
    {
        var before = localTime.PlusMinutes(OffsetBetweenTimingsMinutes * -1);
        var after = localTime.PlusMinutes(OffsetBetweenTimingsMinutes);

        return (localTime, before, after);
    }
}