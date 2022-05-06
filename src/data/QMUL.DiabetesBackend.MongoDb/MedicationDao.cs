namespace QMUL.DiabetesBackend.MongoDb
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Hl7.Fhir.Model;
    using MongoDB.Bson;
    using MongoDB.Driver;
    using DataInterfaces;
    using DataInterfaces.Exceptions;
    using Microsoft.Extensions.Logging;
    using Model;
    using Utils;
    using Task = System.Threading.Tasks.Task;

    /// <summary>
    /// The Mongo Medication Dao
    /// </summary>
    public class MedicationDao : MongoDaoBase, IMedicationDao
    {
        private const string CollectionName = "medication";

        private readonly IMongoCollection<BsonDocument> medicationCollection;
        private readonly ILogger<MedicationDao> logger;

        public MedicationDao(IMongoDatabase database, ILogger<MedicationDao> logger) : base(database)
        {
            this.logger = logger;
            this.medicationCollection = this.Database.GetCollection<BsonDocument>(CollectionName);
        }

        /// <inheritdoc />
        public async Task<PaginatedResult<IEnumerable<Resource>>> GetMedicationList(PaginationRequest paginationRequest,
            string name = null)
        {
            this.logger.LogTrace("Getting all medications...");
            var searchFilter = name == null
                ? FilterDefinition<BsonDocument>.Empty
                : Builders<BsonDocument>.Filter.Regex("code.coding.display", new BsonRegularExpression(name, "i"));
            var resultsFilter = Helpers.GetPaginationFilter(searchFilter, paginationRequest.LastCursorId);
            var result = await this.medicationCollection.Find(resultsFilter)
                .Limit(paginationRequest.Limit)
                .Project(document => Helpers.ToResourceAsync<Medication>(document))
                .ToListAsync();

            Resource[] medications = await Task.WhenAll(result);
            this.logger.LogTrace("Found {Count} medications", medications.Length);
            if (medications.Length == 0)
            {
                return new PaginatedResult<IEnumerable<Resource>> { Results = medications };
            }

            return await Helpers.GetPaginatedResult(this.medicationCollection, searchFilter, medications);
        }

        /// <inheritdoc />
        public async Task<Medication> GetSingleMedication(string id)
        {
            var errorMessage = $"Could not find a medication with ID {id}";
            return await this.GetSingleMedicationOrThrow(id, new NotFoundException(errorMessage));
        }

        /// <inheritdoc />
        public async Task<Medication> CreateMedication(Medication newMedication)
        {
            this.logger.LogDebug("Creating medication...");
            var document = await Helpers.ToBsonDocumentAsync(newMedication);
            await this.medicationCollection.InsertOneAsync(document);

            var newId = document["_id"].ToString();
            this.logger.LogDebug("Medication created with ID: {Id}", newId);
            var errorMessage = $"Could not create medication. ID assigned was: {newId}";
            return await this.GetSingleMedicationOrThrow(newId, new CreateException(errorMessage));
        }

        private async Task<Medication> GetSingleMedicationOrThrow(string id, DataExceptionBase exception)
        {
            var result = this.medicationCollection.Find(Helpers.GetByIdFilter(id));
            var document = await this.GetSingleOrThrow(result, exception);
            return await Helpers.ToResourceAsync<Medication>(document);
        }
    }
}