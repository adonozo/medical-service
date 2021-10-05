namespace QMUL.DiabetesBackend.ServiceInterfaces
{
    using System;
    using System.Threading.Tasks;
    using Hl7.Fhir.Model;
    using Model.Enums;

    /// <summary>
    /// The Observation Service Interface.
    /// </summary>
    public interface IObservationService
    {
        public Task<Observation> CreateObservation(Observation newObservation);

        public Task<Observation> GetSingleObservation(string observationId);
        
        public Task<Bundle> GetAllObservationsFor(string patientId);

        public Task<Bundle> GetObservationsFor(string patientId, CustomEventTiming timing, DateTime dateTime,
            string patientTimezone = "UTC");
    }
}