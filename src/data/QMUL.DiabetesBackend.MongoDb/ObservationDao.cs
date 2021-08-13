using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using MongoDB.Driver;
using QMUL.DiabetesBackend.DataInterfaces;
using QMUL.DiabetesBackend.Model;
using QMUL.DiabetesBackend.MongoDb.Models;
using QMUL.DiabetesBackend.MongoDb.Utils;

namespace QMUL.DiabetesBackend.MongoDb
{
    public class ObservationDao : MongoDaoBase, IObservationDao
    {
        private readonly IMongoCollection<MongoObservation> observationCollection;
        private const string CollectionName = "observation";
        
        public ObservationDao(IDatabaseSettings settings) : base(settings)
        {
            this.observationCollection = this.Database.GetCollection<MongoObservation>(CollectionName);
        }

        public async Task<Observation> CreateObservation(Observation observation)
        {
            var mongoObservation = observation.ToMongoObservation();
            await this.observationCollection.InsertOneAsync(mongoObservation);
            return await this.GetObservation(mongoObservation.Id);
        }

        public async Task<Observation> GetObservation(string observationId)
        {
            var cursor = this.observationCollection.Find(observation => observation.Id == observationId)
                .Project(mongoObservation => mongoObservation.ToObservation());
            return await cursor.FirstOrDefaultAsync();
        }

        public async Task<List<Observation>> GetAllObservationsFor(string patientId)
        {
            var cursor = this.observationCollection.Find(observation =>
                    observation.PatientReference.ReferenceId == patientId)
                .Project(mongoObservation => mongoObservation.ToObservation());
            return await cursor.ToListAsync();
        }

        public async Task<List<Observation>> GetObservationsFor(string patientId, DateTime start, DateTime end)
        {
            var cursor = this.observationCollection.Find(observation =>
                    observation.PatientReference.ReferenceId == patientId
                    && observation.Issued > start
                    && observation.Issued < end)
                .Project(mongoObservation => mongoObservation.ToObservation());
            return await cursor.ToListAsync();
        }
    }
}