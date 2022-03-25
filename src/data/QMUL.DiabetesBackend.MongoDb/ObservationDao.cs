namespace QMUL.DiabetesBackend.MongoDb
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using DataInterfaces;
    using DataInterfaces.Exceptions;
    using Hl7.Fhir.Model;
    using Microsoft.Extensions.Logging;
    using Model.Constants;
    using MongoDB.Bson;
    using MongoDB.Driver;

    /// <summary>
    /// The Observation Dao
    /// </summary>
    public class ObservationDao : MongoDaoBase, IObservationDao
    {
        private readonly IMongoCollection<Observation> observationCollection;
        private const string CollectionName = "observation";
        private readonly ILogger<ObservationDao> logger;

        /// <inheritdoc />
        public ObservationDao(IMongoDatabase database, ILogger<ObservationDao> logger) : base(database)
        {
            this.logger = logger;
            this.observationCollection = this.Database.GetCollection<Observation>(CollectionName);
        }

        /// <inheritdoc />
        public async Task<Observation> CreateObservation(Observation observation)
        {
            this.logger.LogDebug("Creating observation");
            observation.Id = ObjectId.GenerateNewId().ToString();
            await this.observationCollection.InsertOneAsync(observation);
            this.logger.LogDebug("Observation created with ID: {Id}", observation.Id);
            const string errorMessage = "Could not create observation";
            return await this.GetSingleOrThrow(this.observationCollection
                    .Find(mongoObservation => mongoObservation.Id == observation.Id),
                new CreateException(errorMessage),
                () => this.logger.LogWarning(errorMessage));
        }

        /// <inheritdoc />
        public async Task<Observation> GetObservation(string observationId)
        {
            var cursor = this.observationCollection.Find(observation => observation.Id == observationId);
            var errorMessage = $"Could not find observation with ID {observationId}";
            return await this.GetSingleOrThrow(cursor, new NotFoundException(errorMessage),
                () => this.logger.LogWarning("{ErrorMessage}", errorMessage));
        }

        /// <inheritdoc />
        public async Task<List<Observation>> GetAllObservationsFor(string patientId)
        {
            var patientReference = Constants.PatientPath + patientId;
            var cursor = this.observationCollection.Find(observation =>
                    observation.Subject.Reference == patientReference);
            return await cursor.ToListAsync();
        }

        /// <inheritdoc />
        public async Task<List<Observation>> GetObservationsFor(string patientId, DateTime start, DateTime end)
        {
            var patientReference = Constants.PatientPath + patientId;
            var cursor = this.observationCollection.Find(observation =>
                    observation.Subject.Reference == patientReference
                    && observation.Issued > start
                    && observation.Issued < end);
            return await cursor.ToListAsync();
        }
    }
}