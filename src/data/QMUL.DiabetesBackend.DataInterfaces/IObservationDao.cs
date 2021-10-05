namespace QMUL.DiabetesBackend.DataInterfaces
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Hl7.Fhir.Model;

    /// <summary>
    /// The Observation Dao interface.
    /// </summary>
    public interface IObservationDao
    {
        public Task<Observation> CreateObservation(Observation observation);

        public Task<Observation> GetObservation(string observationId);
        
        public Task<List<Observation>> GetAllObservationsFor(string patientId);

        public Task<List<Observation>> GetObservationsFor(string patientId, DateTime start, DateTime end);
    }
}