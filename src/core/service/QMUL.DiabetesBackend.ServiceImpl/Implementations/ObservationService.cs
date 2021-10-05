namespace QMUL.DiabetesBackend.ServiceImpl.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using DataInterfaces;
    using Hl7.Fhir.Model;
    using Model.Enums;
    using ServiceInterfaces;
    using Utils;

    /// <summary>
    /// The Observation Service manages patients' clinical results, e.g., a blood glucose measurement. 
    /// </summary>
    public class ObservationService : IObservationService
    {
        private readonly IPatientDao patientDao;
        private readonly IObservationDao observationDao;
        private const int DefaultOffset = 20;  // The default offset in minutes for search between dates

        public ObservationService(IPatientDao patientDao, IObservationDao observationDao)
        {
            this.patientDao = patientDao;
            this.observationDao = observationDao;
        }

        /// <inheritdoc/>>
        public async Task<Observation> CreateObservation(Observation newObservation)
        {
            await ResourceUtils.ValidateObject(
                () => this.patientDao.GetPatientByIdOrEmail(newObservation.Subject.ElementId),
                "Unable to find patient for the Observation", new KeyNotFoundException());
            var observation = await ResourceUtils.ValidateObject(
                () => this.observationDao.CreateObservation(newObservation),
                "Unable to create Observation", new ArgumentException("Invalid observation", nameof(newObservation)));
            return observation;
        }

        /// <inheritdoc/>>
        public async Task<Observation> GetSingleObservation(string observationId)
        {
            var observation = await ResourceUtils.ValidateObject(
                () => this.observationDao.GetObservation(observationId),
                $"Observation not found: {observationId}", new KeyNotFoundException());
            return observation;
        }

        /// <inheritdoc/>>
        public async Task<Bundle> GetAllObservationsFor(string patientId)
        {
            var patient = await ResourceUtils.ValidateObject(
                () => this.patientDao.GetPatientByIdOrEmail(patientId),
                "Unable to find patient for the Observation", new KeyNotFoundException());
            var observations = await this.observationDao.GetAllObservationsFor(patient.Id);
            var bundle = ResourceUtils.GenerateEmptyBundle();
            bundle.Entry = observations.Select(observation => new Bundle.EntryComponent {Resource = observation})
                .ToList();
            return bundle;
        }

        /// <inheritdoc/>>
        public async Task<Bundle> GetObservationsFor(string patientId, CustomEventTiming timing, DateTime dateTime, string patientTimezone = "UTC")
        {
            if (timing == CustomEventTiming.EXACT)
            {
                return await this.GetObservationsFor(patientId, dateTime);
            }
            
            var patient = await ResourceUtils.ValidateObject(
                () => this.patientDao.GetPatientByIdOrEmail(patientId),
                "Unable to find patient for the Observation", new KeyNotFoundException());
            var (start, end) =
                EventTimingMapper.GetIntervalForPatient(patient, dateTime, timing, patientTimezone, DefaultOffset);

            var observations = await this.observationDao.GetObservationsFor(patient.Id, start, end);
            var bundle = ResourceUtils.GenerateEmptyBundle();
            bundle.Entry = observations.Select(observation => new Bundle.EntryComponent {Resource = observation})
                .ToList();
            return bundle;
        }

        private async Task<Bundle> GetObservationsFor(string patientId, DateTime dateTime)
        {
            var patient = await ResourceUtils.ValidateObject(
                () => this.patientDao.GetPatientByIdOrEmail(patientId),
                "Unable to find patient for the Observation", new KeyNotFoundException());
            var startDate = dateTime.AddMinutes(DefaultOffset * -1);
            var endDate = dateTime.AddMinutes(DefaultOffset);
            var observations = await this.observationDao.GetObservationsFor(patient.Id, startDate, endDate);
            var bundle = ResourceUtils.GenerateEmptyBundle();
            bundle.Entry = observations.Select(observation => new Bundle.EntryComponent {Resource = observation})
                .ToList();
            return bundle;
        }
    }
}