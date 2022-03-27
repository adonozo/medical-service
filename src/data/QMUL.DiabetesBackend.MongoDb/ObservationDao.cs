namespace QMUL.DiabetesBackend.MongoDb
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using DataInterfaces;
    using DataInterfaces.Exceptions;
    using Hl7.Fhir.Model;
    using Microsoft.Extensions.Logging;
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
        public ObservationDao(IMongoDatabase database, ILogger<ObservationDao> logger) : base(database)
        {
            this.logger = logger;
            this.observationCollection = this.Database.GetCollection<MongoObservation>(CollectionName);
        }

        /// <inheritdoc />
        public async Task<Observation> CreateObservation(Observation observation)
        {
            this.logger.LogDebug("Creating observation");
            var newMongoObservation = observation.ToMongoObservation();
            await this.observationCollection.InsertOneAsync(newMongoObservation);
            this.logger.LogDebug("Observation created with ID: {Id}", newMongoObservation.Id);
            const string errorMessage = "Could not create observation";
            return await this.GetSingleOrThrow(this.observationCollection
                    .Find(mongoObservation => mongoObservation.Id == newMongoObservation.Id)
                    .Project(mongoObservation => mongoObservation.ToObservation()),
                new CreateException(errorMessage),
                () => this.logger.LogWarning(errorMessage));
        }

        /// <inheritdoc />
        public async Task<Observation> GetObservation(string observationId)
        {
            var cursor = this.observationCollection.Find(observation => observation.Id == observationId)
                .Project(mongoObservation => mongoObservation.ToObservation());
            var errorMessage = $"Could not find observation with ID {observationId}";
            return await this.GetSingleOrThrow(cursor, new NotFoundException(errorMessage),
                () => this.logger.LogWarning("{ErrorMessage}", errorMessage));
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