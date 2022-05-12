namespace QMUL.DiabetesBackend.ServiceImpl.Implementations
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using DataInterfaces;
    using Hl7.Fhir.Model;
    using Model;
    using Model.Enums;
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

        /// <inheritdoc/>>
        public async Task<Observation> CreateObservation(Observation newObservation, string patientId = null)
        {
            // Check if the patient exists
            var patient = await ExceptionHandler.ExecuteAndHandleAsync(async () =>
                await this.patientDao.GetPatientByIdOrEmail(patientId), this.logger);
            newObservation.Subject.SetPatientReference(patientId);
            newObservation.Subject.Display = patient.Name[0].Family;
            var observation = await ExceptionHandler.ExecuteAndHandleAsync(async () =>
                await this.observationDao.CreateObservation(newObservation), this.logger);
            this.logger.LogDebug("Observation created with ID {Id}", observation.Id);
            return observation;
        }

        /// <inheritdoc/>>
        public async Task<Observation> GetObservation(string observationId)
        {
            var observation = await ExceptionHandler.ExecuteAndHandleAsync(async () =>
                await this.observationDao.GetObservation(observationId), this.logger);
            this.logger.LogDebug("Observation found: {Id}", observationId);
            return observation;
        }

        /// <inheritdoc/>>
        public async Task<PaginatedResult<Bundle>> GetObservations(string patientId, PaginationRequest paginationRequest)
        {
            var patient = await ExceptionHandler.ExecuteAndHandleAsync(async () =>
                await this.patientDao.GetPatientByIdOrEmail(patientId), this.logger);
            var observations = await this.observationDao.GetAllObservationsFor(patient.Id, paginationRequest);
            
            var paginateResult = observations.ToBundleResult();
            this.logger.LogDebug("Found {Count} observations", observations.Results.Count());
            return paginateResult;
        }

        /// <inheritdoc/>>
        public async Task<PaginatedResult<Bundle>> GetObservationsFor(string patientId, CustomEventTiming timing, DateTime dateTime,
            PaginationRequest paginationRequest, string patientTimezone = "UTC")
        {
            var patient = await ExceptionHandler.ExecuteAndHandleAsync(async () =>
                await this.patientDao.GetPatientByIdOrEmail(patientId), this.logger);
            var timingPreferences = patient.GetTimingPreference();

            DateTimeOffset start, end;
            if (timing == CustomEventTiming.EXACT)
            {
                start = dateTime.AddMinutes(DefaultOffset * -1);
                end = dateTime.AddMinutes(DefaultOffset);
            }
            else
            {
                (start, end) =
                    EventTimingMapper.GetIntervalForPatient(timingPreferences, dateTime, timing, patientTimezone, DefaultOffset);
            }

            var observations = await this.observationDao.GetObservationsFor(patient.Id, start.UtcDateTime,
                end.UtcDateTime, paginationRequest);
            var paginatedResult = observations.ToBundleResult();
            this.logger.LogDebug("Observations found for {PatientId}: {Count}", patientId,
                observations.Results.Count());
            return paginatedResult;
        }

        public async Task<Observation> UpdateObservation(string id, Observation updatedObservation)
        {
            await ExceptionHandler.ExecuteAndHandleAsync(async () => 
                await this.observationDao.GetObservation(id), this.logger);

            updatedObservation.Id = id;
            return await ExceptionHandler.ExecuteAndHandleAsync(async () =>
                await this.observationDao.UpdateObservation(id, updatedObservation), this.logger);
        }

        public async Task<Observation> UpdateValue(string observationId, DataType value)
        {
            var observation = await ExceptionHandler.ExecuteAndHandleAsync(async () => 
                await this.observationDao.GetObservation(observationId), this.logger);

            observation.Value = value;
            return await ExceptionHandler.ExecuteAndHandleAsync(async () =>
                await this.observationDao.UpdateObservation(observationId, observation), this.logger);
        }

        public async Task<bool> DeleteObservation(string id)
        {
            await ExceptionHandler.ExecuteAndHandleAsync(async () => 
                await this.observationDao.GetObservation(id), this.logger);

            return await ExceptionHandler.ExecuteAndHandleAsync(async () =>
                await this.observationDao.DeleteObservation(id), this.logger);
        }
    }
}