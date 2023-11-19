namespace QMUL.DiabetesBackend.Service;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataInterfaces;
using Hl7.Fhir.Model;
using Microsoft.Extensions.Logging;
using Model;
using Model.Alexa;
using Model.Enums;
using Model.Exceptions;
using Model.Extensions;
using Model.Utils;
using NodaTime;
using ServiceInterfaces;
using Utils;
using Instant = NodaTime.Instant;
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
    private readonly IAlexaDao alexaDao;
    private readonly IClock clock;
    private readonly ILogger<AlexaService> logger;

    public AlexaService(IPatientDao patientDao,
        IMedicationRequestDao medicationRequestDao,
        IServiceRequestDao serviceRequestDao,
        IAlexaDao alexaDao,
        ILogger<AlexaService> logger,
        IClock clock)
    {
        this.patientDao = patientDao;
        this.medicationRequestDao = medicationRequestDao;
        this.serviceRequestDao = serviceRequestDao;
        this.alexaDao = alexaDao;
        this.logger = logger;
        this.clock = clock;
    }

    /// <inheritdoc/>
    public async Task<Result<Bundle, MedicationRequest>> SearchMedicationRequests(string patientEmailOrId,
        LocalDate date,
        bool onlyInsulin,
        CustomEventTiming? timing = CustomEventTiming.ALL_DAY,
        string? timezone = "UTC")
    {
        var patient = await ResourceUtils.GetResourceOrThrowAsync(async () =>
            await this.patientDao.GetPatientByIdOrEmail(patientEmailOrId), new NotFoundException());

        var activeRequestsResult = await this.medicationRequestDao.GetActiveMedicationRequests(
            patient.Id,
            PaginationRequest.FirstPaginatedResults,
            onlyInsulin);

        var medicationRequests = activeRequestsResult.Results.ToList();
        var requestsNeedStartDateOrTime = medicationRequests
            .Where(request => request.NeedsStartDate() || request.NeedsStartTime())
            .ToList();

        if (requestsNeedStartDateOrTime.Any())
        {
            return Result<Bundle, MedicationRequest>.Fail(requestsNeedStartDateOrTime.First());
        }

        var dateFilter = EventTimingMapper.TimingIntervalForPatient(
            localDate: date,
            timing: timing ?? CustomEventTiming.ALL_DAY,
            timezone: timezone ?? "UTC");

        var results = this.FindMedicationRequestsInDate(activeRequestsResult.Results, dateFilter);

        var bundle = ResourceUtils.GenerateSearchBundle(results);
        return Result<Bundle, MedicationRequest>.Success(bundle);
    }

    /// <inheritdoc/>>
    public async Task<Result<Bundle, ServiceRequest>> SearchActiveServiceRequests(string patientEmailOrId,
        LocalDate? startDate = null,
        LocalDate? endDate = null)
    {
        var patient = await ResourceUtils.GetResourceOrThrowAsync(async () =>
            await this.patientDao.GetPatientByIdOrEmail(patientEmailOrId), new NotFoundException());

        var serviceRequests = await this.serviceRequestDao.GetActiveServiceRequests(patient.Id);
        var requestNeedStartDate = serviceRequests
            .Where(request => request.NeedsStartDate() || request.NeedsStartTime())
            .ToList();

        if (requestNeedStartDate.Any())
        {
            return Result<Bundle, ServiceRequest>.Fail(requestNeedStartDate.First());
        }

        var startInterval = startDate is null
            ? this.clock.GetCurrentInstant()
            : DateUtils.InstantFromUtcDate(startDate.Value);
        Instant? endInterval = endDate is null
            ? null
            : DateUtils.InstantFromUtcDate(endDate.Value);
        var interval = new Interval(startInterval, endInterval);

        var results = serviceRequests
            .Where(request => ResourceUtils.ServiceRequestOccursInDate(request, interval));

        var bundle = ResourceUtils.GenerateSearchBundle(results);
        return Result<Bundle, ServiceRequest>.Success(bundle);
    }

    /// <inheritdoc/>
    public async Task<bool> UpsertDosageStartDateTime(string patientIdOrEmail,
        string dosageId,
        LocalDate startDate,
        LocalTime? localTime = null)
    {
        var patient = await ResourceUtils.GetResourceOrThrowAsync(
            () => this.patientDao.GetPatientByIdOrEmail(patientIdOrEmail),
            new ValidationException($"The patient {patientIdOrEmail} was not found"));

        var internalPatient = patient.ToInternalPatient();
        await this.SetDosageStartDate(internalPatient, dosageId, startDate, localTime);
        this.logger.LogDebug("Dosage start date updated for {IdOrEmail}: {DosageId}, {DateTime}", patientIdOrEmail,
            dosageId, startDate);
        return true;
    }

    /// <inheritdoc/>
    public async Task<bool> UpsertServiceRequestStartDate(string patientIdOrEmail, string serviceRequestId,
        LocalDate startDate)
    {
        await ResourceUtils.GetResourceOrThrowAsync(
            () => this.patientDao.GetPatientByIdOrEmail(patientIdOrEmail),
            new ValidationException($"The patient {patientIdOrEmail} was not found"));

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

    /// <inheritdoc/>
    public async Task<Result<AlexaRequest?, string>> GetLastRequest(string patientIdOrEmail, string deviceId)
    {
        var lastRequestResult = await this.GetLastRequest(deviceId);
        if (!lastRequestResult.Succeded)
        {
            return Result<AlexaRequest?, string>.Fail("Could not retrieve the last request");
        }

        var isRequestInserted = await this.alexaDao.InsertRequest(new AlexaRequest
        {
            DeviceId = deviceId,
            Timestamp = this.clock.GetCurrentInstant(),
            UserId = patientIdOrEmail
        });

        return !isRequestInserted
            ? Result<AlexaRequest?, string>.Fail("Could not insert the request")
            : Result<AlexaRequest?, string>.Success(lastRequestResult.AlexaRequest);
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

        var timing = medicationRequest.DosageInstruction[index].Timing;

        if (timing.NeedsStartDate())
        {
            timing.SetStartDate(startDate);
            timing.RemoveNeedsStartDateFlag();
        }

        if (startTime.HasValue)
        {
            timing.SetStartTime(startTime.Value);
            timing.RemoveNeedsStartTimeFlag();
        }

        await this.medicationRequestDao.UpdateMedicationRequest(medicationRequest.Id, medicationRequest);
    }

    private IEnumerable<MedicationRequest> FindMedicationRequestsInDate(
        IEnumerable<MedicationRequest> requests,
        Interval dateFilter)
    {
        var results = new List<MedicationRequest>();
        foreach (var request in requests)
        {
            var dosagesInFilter = ResourceUtils.FilterDosagesOutsideFilter(request, dateFilter);
            if (!dosagesInFilter.Any())
            {
                continue;
            }

            request.DosageInstruction = dosagesInFilter;
            results.Add(request);
        }

        return results;
    }

    private async Task<(bool Succeded, AlexaRequest? AlexaRequest)> GetLastRequest(string deviceId)
    {
        try
        {
            var lastRequest = await this.alexaDao.GetLastRequest(deviceId);
            return (true, lastRequest);
        }
        catch (Exception e)
        {
            this.logger.LogError(e, $"Could not retrieve the last request for");
            return (false, null);
        }
    }
}
