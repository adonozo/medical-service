using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hl7.Fhir.Model;

namespace QMUL.DiabetesBackend.DataInterfaces
{
    public interface IObservationDao
    {
        public Task<Observation> CreateObservation(Observation observation);

        public Task<Observation> GetObservation(string observationId);

        public Task<List<Observation>> GetObservationsFor(string patientId, DateTime start, DateTime end);
    }
}