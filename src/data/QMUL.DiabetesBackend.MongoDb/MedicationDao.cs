namespace QMUL.DiabetesBackend.MongoDb
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Hl7.Fhir.Model;
    using MongoDB.Bson;
    using MongoDB.Driver;
    using DataInterfaces;
    using Microsoft.Extensions.Logging;
    using Model;

    /// <summary>
    /// The Mongo Medication Dao
    /// </summary>
    public class MedicationDao : MongoDaoBase, IMedicationDao
    {
        private readonly IMongoCollection<Medication> medicationCollection;
        private readonly ILogger<MedicationDao> logger;
        private const string CollectionName = "medication";

        public MedicationDao(IDatabaseSettings settings, ILogger<MedicationDao> logger) : base(settings)
        {
            this.logger = logger;
            this.medicationCollection = this.Database.GetCollection<Medication>(CollectionName);
        }

        /// <inheritdoc />
        public async Task<List<Medication>> GetMedicationList()
        {
            var result =  this.medicationCollection.Find(FilterDefinition<Medication>.Empty);
            return await result.ToListAsync();
        }

        /// <inheritdoc />
        public async Task<Medication> GetSingleMedication(string id)
        {
            var result = this.medicationCollection.Find(medication => medication.Id == id);
            return await result.FirstOrDefaultAsync();
        }

        /// <inheritdoc />
        public async Task<Medication> CreateMedication(Medication newMedication)
        {
            this.logger.LogDebug("Creating medication...");
            newMedication.Id = ObjectId.GenerateNewId().ToString();
            await this.medicationCollection.InsertOneAsync(newMedication);
            this.logger.LogDebug("Medication created with ID: {Id}", newMedication.Id);
            return await this.GetSingleMedication(newMedication.Id);
        }
    }
}