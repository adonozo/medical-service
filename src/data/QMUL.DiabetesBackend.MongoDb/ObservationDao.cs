namespace QMUL.DiabetesBackend.MongoDb
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using DataInterfaces;
    using DataInterfaces.Exceptions;
    using Hl7.Fhir.Model;
    using Microsoft.Extensions.Logging;
    using Model;
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
        public async Task<PaginatedResult<IEnumerable<Resource>>> GetAllObservationsFor(string patientId,
            PaginationRequest paginationRequest)
        {
            var searchFilter = Helpers.GetPatientReferenceFilter(patientId);
            var resultsFilter = Helpers.GetPaginationFilter(searchFilter, paginationRequest.LastCursorId);

            var results = await this.observationCollection.Find(resultsFilter)
                .Limit(paginationRequest.Limit)
                .Project(document => this.ProjectToObservation(document))
                .ToListAsync();
            Resource[] observations = await Task.WhenAll(results);

            if (observations.Length == 0)
            {
                return new PaginatedResult<IEnumerable<Resource>> { Results = observations };
            }

            return await Helpers.GetPaginatedResult(this.observationCollection, searchFilter, observations);
        }

        /// <inheritdoc />
        public async Task<PaginatedResult<IEnumerable<Resource>>> GetObservationsFor(string patientId, DateTime start,
            DateTime end, PaginationRequest paginationRequest)
        {
            var searchFilter = Builders<BsonDocument>.Filter.And(
                Helpers.GetPatientReferenceFilter(patientId),
                Builders<BsonDocument>.Filter.Gt("issued", start),
                Builders<BsonDocument>.Filter.Lt("issued", end));

            var resultsFilter = Helpers.GetPaginationFilter(searchFilter, paginationRequest.LastCursorId);
            var result = await this.observationCollection.Find(resultsFilter)
                .Limit(paginationRequest.Limit)
                .Project(document => this.ProjectToObservation(document))
                .ToListAsync();

            Resource[] observations = await Task.WhenAll(result);
            if (observations.Length == 0)
            {
                return new PaginatedResult<IEnumerable<Resource>> { Results = observations };
            }

            return await Helpers.GetPaginatedResult(this.observationCollection, searchFilter, observations);
        }

        private async Task<Observation> ProjectToObservation(BsonDocument document)
        {
            document["issued"] = document["issued"].ToString();
            return await Helpers.ToResourceAsync<Observation>(document);
        }
    }
}