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
    public async Task<Result<Bundle, MedicationRequest>> SearchMedicationRequests(string patientEmailOrId,
        LocalDate date,
        bool onlyInsulin,
        CustomEventTiming? timing = CustomEventTiming.ALL_DAY,
        string? timezone = "UTC")
    {
        await ResourceUtils.GetResourceOrThrowAsync(async () =>
            await this.patientDao.GetPatientByIdOrEmail(patientEmailOrId), new NotFoundException());

        var activeRequestsResult = await this.medicationRequestDao.GetActiveMedicationRequests(
            patientEmailOrId,
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

    /// <inheritdoc/>
    public async Task<Result<Bundle, ServiceRequest>> SearchServiceRequests(string patientEmailOrId,
        LocalDate dateTime,
        CustomEventTiming timing,
        string timezone = "UTC")
    {
        await ResourceUtils.GetResourceOrThrowAsync(async () =>
            await this.patientDao.GetPatientByIdOrEmail(patientEmailOrId), new NotFoundException());

        var serviceRequests = await this.serviceRequestDao.GetActiveServiceRequests(patientEmailOrId);
        var requestNeedStartDate = serviceRequests
            .Where(request => request.NeedsStartDate() || request.NeedsStartTime())
            .ToList();

        if (requestNeedStartDate.Any())
        {
            return Result<Bundle, ServiceRequest>.Fail(requestNeedStartDate.First());
        }

        var dateFilter = EventTimingMapper.TimingIntervalForPatient(
            localDate: dateTime,
            timing: timing,
            timezone: timezone);

        var results = serviceRequests
            .Where(request => ResourceUtils.ServiceRequestOccursInDate(request, dateFilter));
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
}