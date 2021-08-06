using System;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using QMUL.DiabetesBackend.Model.Enums;

namespace QMUL.DiabetesBackend.ServiceInterfaces
{
    public interface IObservationService
    {
        public Task<Observation> CreateObservation(Observation newObservation);

        public Task<Observation> GetSingleObservation(string observationId);

        public Task<Bundle> GetObservationsFor(string patientId, DateTime dateTime);

        public Task<Bundle> GetObservationsFor(string patientId, CustomEventTiming timing, DateTime dateTime,
            string patientTimezone = "UTC");
    }
}