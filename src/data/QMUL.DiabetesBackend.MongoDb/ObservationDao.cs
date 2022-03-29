namespace QMUL.DiabetesBackend.MongoDb
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using DataInterfaces;
    using DataInterfaces.Exceptions;
    using Hl7.Fhir.Model;
    using Microsoft.Extensions.Logging;
    using MongoDB.Bson;
    using MongoDB.Driver;
    using Utils;
    using Task = System.Threading.Tasks.Task;

    /// <summary>
    /// The Observation Dao
    /// </summary>
    public class ObservationDao : MongoDaoBase, IObservationDao
    {
        private const string CollectionName = "observation";

        private readonly IMongoCollection<BsonDocument> observationCollection;
        private readonly ILogger<ObservationDao> logger;

        /// <inheritdoc />
        public ObservationDao(IMongoDatabase database, ILogger<ObservationDao> logger) : base(database)
        {
            this.logger = logger;
            this.observationCollection = this.Database.GetCollection<BsonDocument>(CollectionName);
        }

        /// <inheritdoc />
        public async Task<Observation> CreateObservation(Observation observation)
        {
            this.logger.LogDebug("Creating observation...");
            var document = await Helpers.ToBsonDocumentAsync(observation);
            Helpers.SetBsonDateTimeValue(document, "issued", observation.Issued);
            await this.observationCollection.InsertOneAsync(document);

            var newId = document["_id"].ToString();
            this.logger.LogDebug("Observation created with ID: {Id}", newId);
            const string errorMessage = "Could not create observation";
            document = await this.GetSingleOrThrow(this.observationCollection.Find(Helpers.GetByIdFilter(newId)),
                new CreateException(errorMessage));
            return await this.ProjectToObservation(document);
        }

        /// <inheritdoc />
        public async Task<Observation> GetObservation(string observationId)
        {
            var cursor = this.observationCollection.Find(Helpers.GetByIdFilter(observationId));
            var errorMessage = $"Could not find observation with ID {observationId}";
            var document = await this.GetSingleOrThrow(cursor, new NotFoundException(errorMessage));
            return await this.ProjectToObservation(document);
        }

        /// <inheritdoc />
        public async Task<IList<Observation>> GetAllObservationsFor(string patientId)
        {
            var results = await this.observationCollection.Find(Helpers.GetPatientReferenceFilter(patientId))
                .Project(document => this.ProjectToObservation(document))
                .ToListAsync();
            return await Task.WhenAll(results);
        }

        /// <inheritdoc />
        public async Task<IList<Observation>> GetObservationsFor(string patientId, DateTime start, DateTime end)
        {
            var filter = Builders<BsonDocument>.Filter.And(
                Helpers.GetPatientReferenceFilter(patientId),
                Builders<BsonDocument>.Filter.Gt("issued", start),
                Builders<BsonDocument>.Filter.Lt("issued", end));

            var result = await this.observationCollection.Find(filter)
                .Project(document => this.ProjectToObservation(document))
                .ToListAsync();
            return await Task.WhenAll(result);
        }

        private async Task<Observation> ProjectToObservation(BsonDocument document)
        {
            document["issued"] = document["issued"].ToString();
            return await Helpers.ToResourceAsync<Observation>(document);
        }
    }
}