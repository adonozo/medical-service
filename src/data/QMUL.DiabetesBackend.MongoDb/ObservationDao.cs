namespace QMUL.DiabetesBackend.MongoDb
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using DataInterfaces;
    using Hl7.Fhir.Model;
    using Microsoft.Extensions.Logging;
    using Model;
    using Models;
    using MongoDB.Driver;
    using Utils;

    /// <summary>
    /// The Observation Dao
    /// </summary>
    public class ObservationDao : MongoDaoBase, IObservationDao
    {
        private readonly IMongoCollection<MongoObservation> observationCollection;
        private const string CollectionName = "observation";
        private readonly ILogger<ObservationDao> logger;
        
        /// <inheritdoc />
        public ObservationDao(IDatabaseSettings settings, ILogger<ObservationDao> logger) : base(settings)
        {
            this.logger = logger;
            this.observationCollection = this.Database.GetCollection<MongoObservation>(CollectionName);
        }

        /// <inheritdoc />
        public async Task<Observation> CreateObservation(Observation observation)
        {
            this.logger.LogDebug("Creating observation");
            var mongoObservation = observation.ToMongoObservation();
            await this.observationCollection.InsertOneAsync(mongoObservation);
            this.logger.LogDebug("Observation created with ID: {Id}", mongoObservation.Id);
            return await this.GetObservation(mongoObservation.Id);
        }

        /// <inheritdoc />
        public async Task<Observation> GetObservation(string observationId)
        {
            var cursor = this.observationCollection.Find(observation => observation.Id == observationId)
                .Project(mongoObservation => mongoObservation.ToObservation());
            return await cursor.FirstOrDefaultAsync();
        }

        /// <inheritdoc />
        public async Task<List<Observation>> GetAllObservationsFor(string patientId)
        {
            var cursor = this.observationCollection.Find(observation =>
                    observation.PatientReference.ReferenceId == patientId)
                .Project(mongoObservation => mongoObservation.ToObservation());
            return await cursor.ToListAsync();
        }

        /// <inheritdoc />
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