namespace QMUL.DiabetesBackend.Service;

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using DataInterfaces;
using Hl7.Fhir.Model;
using Model;
using Model.Enums;
using Model.Exceptions;
using Model.Extensions;
using ServiceInterfaces;
using Utils;

/// <summary>
/// The Observation Service manages patients' clinical results, e.g., a blood glucose measurement. 
/// </summary>
public class ObservationService : IObservationService
{
    private readonly IPatientDao patientDao;
    private readonly IObservationDao observationDao;
    private readonly ILogger<ObservationService> logger;
    private const int DefaultOffset = 20; // The default offset in minutes for search between dates

    public ObservationService(IPatientDao patientDao, IObservationDao observationDao,
        ILogger<ObservationService> logger)
    {
        this.patientDao = patientDao;
        this.observationDao = observationDao;
        this.logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Observation> CreateObservation(Observation newObservation, string? patientId = null)
    {
        patientId ??= newObservation.Subject.GetPatientIdFromReference();
        var patientNotFoundException = new ValidationException($"Patient not found: {patientId}");
        var patient = await ResourceUtils.GetResourceOrThrow(async () =>
            await this.patientDao.GetPatientByIdOrEmail(patientId), patientNotFoundException);

        newObservation.Subject.SetPatientReference(patient.Id);
        newObservation.Subject.Display = patient.Name[0].Family;

        var observation = await this.observationDao.CreateObservation(newObservation);
        this.logger.LogDebug("Observation created with ID {Id}", observation.Id);
        return observation;
    }

    /// <inheritdoc/>
    public Task<Observation?> GetObservation(string observationId)
    {
        return this.observationDao.GetObservation(observationId);
    }

    /// <inheritdoc/>
    public async Task<PaginatedResult<Bundle>> GetObservations(string patientId,
        PaginationRequest paginationRequest)
    {
        var observations = await this.observationDao.GetAllObservationsFor(patientId, paginationRequest);
        var paginateResult = observations.ToBundleResult();

        this.logger.LogDebug("Found {Count} observations", observations.Results.Count());
        return paginateResult;
    }

    /// <inheritdoc/>
    public async Task<PaginatedResult<Bundle>> GetObservationsFor(string patientId,
        CustomEventTiming timing,
        DateTime dateTime,
        PaginationRequest paginationRequest,
        string patientTimezone = "UTC")
    {
        var patientNotFoundException = new NotFoundException($"Patient not found: {patientId}");
        var patient = await ResourceUtils.GetResourceOrThrow(async () =>
            await this.patientDao.GetPatientByIdOrEmail(patientId), patientNotFoundException);
        var timingPreferences = patient.GetTimingPreference();

        DateTimeOffset start, end;
        if (timing == CustomEventTiming.EXACT)
        {
            start = dateTime.AddMinutes(DefaultOffset * -1);
            end = dateTime.AddMinutes(DefaultOffset);
        }
        else
        {
            (start, end) = EventTimingMapper.GetIntervalForPatient(
                preferences: timingPreferences,
                startTime: dateTime,
                timing: timing,
                timezone: patientTimezone,
                defaultOffset: DefaultOffset);
        }

        var observations = await this.observationDao.GetObservationsFor(
            patient.Id,
            start.UtcDateTime,
            end.UtcDateTime,
            paginationRequest);
        var paginatedResult = observations.ToBundleResult();
        this.logger.LogDebug("Observations found for {PatientId}: {Count}", patientId,
            observations.Results.Count());
        return paginatedResult;
    }

    /// <inheritdoc/>
    public async Task<Observation> UpdateObservation(string id, Observation updatedObservation)
    {
        var observationNotFoundException = new NotFoundException($"Observation not found: {id}");
        await ResourceUtils.GetResourceOrThrow(async () =>
            await this.observationDao.GetObservation(id), observationNotFoundException);

        var patientId = updatedObservation.Subject.GetPatientIdFromReference();
        var patientNotFoundException = new ValidationException($"Patient not found: {patientId}");
        await ResourceUtils.GetResourceOrThrow(async () =>
            await this.patientDao.GetPatientByIdOrEmail(patientId), patientNotFoundException);

        updatedObservation.Id = id;
        return await this.observationDao.UpdateObservation(id, updatedObservation);
    }

    /// <inheritdoc/>
    public async Task<Observation> UpdateValue(string observationId, DataType value)
    {
        var observationNotFoundException = new NotFoundException($"Observation not found: {observationId}");
        var observation = await ResourceUtils.GetResourceOrThrow(async () =>
            await this.observationDao.GetObservation(observationId), observationNotFoundException);

        observation.Value = value;
        return await this.observationDao.UpdateObservation(observationId, observation);
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteObservation(string id)
    {
        var observationNotFoundException = new NotFoundException($"Observation not found: {id}");
        await ResourceUtils.GetResourceOrThrow(async () =>
            await this.observationDao.GetObservation(id), observationNotFoundException);

        return await this.observationDao.DeleteObservation(id);
    }
}