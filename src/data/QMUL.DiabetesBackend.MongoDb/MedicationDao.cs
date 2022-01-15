namespace QMUL.DiabetesBackend.MongoDb
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Hl7.Fhir.Model;
    using MongoDB.Bson;
    using MongoDB.Driver;
    using DataInterfaces;
    using DataInterfaces.Exceptions;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// The Mongo Medication Dao
    /// </summary>
    public class MedicationDao : MongoDaoBase, IMedicationDao
    {
        private readonly IMongoCollection<Medication> medicationCollection;
        private readonly ILogger<MedicationDao> logger;
        private const string CollectionName = "medication";

        public MedicationDao(IMongoDatabase database, ILogger<MedicationDao> logger) : base(database)
        {
            this.logger = logger;
            this.medicationCollection = this.Database.GetCollection<Medication>(CollectionName);
        }

        /// <inheritdoc />
        public async Task<List<Medication>> GetMedicationList()
        {
            this.logger.LogTrace("Getting all medications");
            var result =  this.medicationCollection.Find(FilterDefinition<Medication>.Empty);
            return await result.ToListAsync();
        }

        /// <inheritdoc />
        public async Task<Medication> GetSingleMedication(string id)
        {
            var errorMessage = $"Could not find a medication with ID {id}";
            return await this.GetSingleMedicationOrThrow(id, new NotFoundException(errorMessage),
                () => this.logger.LogWarning("{ErrorMessage}", errorMessage));
        }

        /// <inheritdoc />
        public async Task<Medication> CreateMedication(Medication newMedication)
        {
            this.logger.LogDebug("Creating medication...");
            newMedication.Id = ObjectId.GenerateNewId().ToString();
            await this.medicationCollection.InsertOneAsync(newMedication);
            this.logger.LogDebug("Medication created with ID: {Id}", newMedication.Id);
            var errorMessage = $"Could not create medication. ID assigned was: {newMedication.Id}";
            return await this.GetSingleMedicationOrThrow(newMedication.Id, new CreateException(errorMessage),
                () => this.logger.LogWarning("{ErrorMessage}", errorMessage));
        }

        private async Task<Medication> GetSingleMedicationOrThrow(string id, DataExceptionBase exception, Action fallback)
        {
            var result = this.medicationCollection.Find(medication => medication.Id == id);
            return await this.GetSingleOrThrow(result, exception, fallback);
        }
    }
}