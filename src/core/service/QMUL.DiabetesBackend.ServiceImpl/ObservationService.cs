using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using QMUL.DiabetesBackend.DataInterfaces;
using QMUL.DiabetesBackend.Model.Enums;
using QMUL.DiabetesBackend.ServiceImpl.Utils;
using QMUL.DiabetesBackend.ServiceInterfaces;

namespace QMUL.DiabetesBackend.ServiceImpl
{
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

        public async Task<Observation> GetSingleObservation(string observationId)
        {
            var observation = await ResourceUtils.ValidateObject(
                () => this.observationDao.GetObservation(observationId),
                $"Observation not found: {observationId}", new KeyNotFoundException());
            return observation;
        }

        public async Task<Bundle> GetObservationsFor(string patientId, DateTime dateTime)
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

        public async Task<Bundle> GetObservationsFor(string patientId, CustomEventTiming timing, DateTime dateTime)
        {
            if (timing == CustomEventTiming.EXACT)
            {
                return await this.GetObservationsFor(patientId, dateTime);
            }
            
            var patient = await ResourceUtils.ValidateObject(
                () => this.patientDao.GetPatientByIdOrEmail(patientId),
                "Unable to find patient for the Observation", new KeyNotFoundException());
            DateTime start;
            DateTime end;
            if (patient.ExactEventTimes.ContainsKey(timing))
            {
                start = patient.ExactEventTimes[timing].AddMinutes(DefaultOffset * -1);
                start = dateTime.Date.AddHours(start.Hour).AddMinutes(start.Minute);
                end = patient.ExactEventTimes[timing].AddMinutes(DefaultOffset);
                end = dateTime.Date.AddHours(end.Hour).AddMinutes(end.Minute);
            }
            else
            {
                (start, end) = EventTimingMapper.GetIntervalFromCustomEventTiming(dateTime, timing);
            }

            var observations = await this.observationDao.GetObservationsFor(patient.Id, start, end);
            var bundle = ResourceUtils.GenerateEmptyBundle();
            bundle.Entry = observations.Select(observation => new Bundle.EntryComponent {Resource = observation})
                .ToList();
            return bundle;
        }
    }
}