namespace QMUL.DiabetesBackend.MongoDb
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using DataInterfaces;
    using DataInterfaces.Exceptions;
    using Hl7.Fhir.Model;
    using Microsoft.Extensions.Logging;
    using Model;
    using Model.Extensions;
    using MongoDB.Bson;
    using MongoDB.Driver;
    using Utils;
    using Task = System.Threading.Tasks.Task;

    /// <summary>
    /// The Patient Dao.
    /// </summary>
    public class PatientDao : MongoDaoBase, IPatientDao
    {
        private const string CollectionName = "patient";

        private readonly ILogger<PatientDao> logger;
        private readonly IMongoCollection<BsonDocument> patientCollection;

        public PatientDao(IMongoDatabase database, ILogger<PatientDao> logger) : base(database)
        {
            this.logger = logger;
            this.Database.GetCollection<Patient>(CollectionName);
            this.patientCollection = this.Database.GetCollection<BsonDocument>(CollectionName);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Patient>> GetPatients()
        {
            var results = await this.patientCollection.Find(FilterDefinition<BsonDocument>.Empty)
                .Project(document => Helpers.ToResourceAsync<Patient>(document))
                .ToListAsync();
            return await Task.WhenAll(results);
        }

        /// <inheritdoc />
        public async Task<Patient> CreatePatient(Patient newPatient)
        {
            this.logger.LogInformation("Inserting patient...");
            var document = await this.PatientToBsonDocument(newPatient);
            await this.patientCollection.InsertOneAsync(document);

            var newId = document["_id"].ToString();
            this.logger.LogInformation("Patient created with ID: {Id}", newId);
            return await this.GetSinglePatientOrThrow(newId);
        }

        /// <inheritdoc />
        public async Task<Patient> GetPatientByIdOrEmail(string idOrEmail)
        {
            var filter = ObjectId.TryParse(idOrEmail, out _) ? Helpers.GetByIdFilter(idOrEmail)
                : Builders<BsonDocument>.Filter.Eq("email", idOrEmail);
            var error = $"Could not find patient with ID or email {idOrEmail}";
            var bsonPatient =
                await this.GetSingleOrThrow(this.patientCollection.Find(filter), new NotFoundException(error));
            return await Helpers.ToResourceAsync<Patient>(bsonPatient);
        }

        /// <inheritdoc />
        public async Task<Patient> UpdatePatient(Patient actualPatient)
        {
            logger.LogInformation("Updating patient with ID: {Id}", actualPatient.Id);
            var bson = await this.PatientToBsonDocument(actualPatient);
            var result = await this.patientCollection.ReplaceOneAsync(Helpers.GetByIdFilter(actualPatient.Id),
                bson, new ReplaceOptions {IsUpsert = true});

            var errorMessage = $"Could not update patient with ID {actualPatient.Id}";
            this.CheckAcknowledgedOrThrow(result.IsAcknowledged, new UpdateException(errorMessage));
            logger.LogInformation("Patient with ID {Id} updated", actualPatient.Id);
            return await this.GetSinglePatientOrThrow(actualPatient.Id);
        }

        /// <inheritdoc />
        public async Task<Patient> PatchPatient(InternalPatient actualPatient, Patient oldPatient)
        {
            logger.LogInformation("Updating patient with ID: {Id}", actualPatient.Id);
            var updatedPatient = await this.SetPatientData(actualPatient, oldPatient);
            return await UpdatePatient(updatedPatient);
        }

        private async Task<Patient> GetSinglePatientOrThrow(string id)
        {
            var cursor = this.patientCollection.Find(Helpers.GetByIdFilter(id));
            const string errorMessage = "Could not find patient.";
            var bsonDocument = await this.GetSingleOrThrow(cursor, new CreateException(errorMessage),
                () => this.logger.LogWarning("{ErrorMessage}", errorMessage));
            return await Helpers.ToResourceAsync<Patient>(bsonDocument);
        }

        private async Task<BsonDocument> PatientToBsonDocument(Patient patient)
        {
            var bson = await Helpers.ToBsonDocumentAsync(patient);
            bson.Add("email", patient.GetEmailExtension());
            return bson;
        }
        
        private async Task<Patient> SetPatientData(InternalPatient patient, Patient oldPatient)
        {
            if (!string.IsNullOrEmpty(patient.LastName))
            {
                oldPatient.Name[0].Family = patient.LastName;
            }
            if (!string.IsNullOrEmpty(patient.FirstName))
            {
                oldPatient.Name[0].Given = new[] {patient.FirstName};
            }
            if (!string.IsNullOrEmpty(patient.Email))
            {
                oldPatient.SetEmailExtension(patient.Email);
            }
            if (!string.IsNullOrEmpty(patient.AlexaUserId))
            {
                oldPatient.SetAlexaIdExtension(patient.AlexaUserId);
            }
            if (patient.Phones != null)
            {
                var phonesTask = patient.Phones.Select(Helpers.ToDataTypeAsync<ContactPoint>);
                var phones = await Task.WhenAll(phonesTask);
                oldPatient.Telecom = phones.ToList();
            }

            oldPatient.Gender = patient.Gender ?? oldPatient.Gender;
            oldPatient.BirthDate = patient.BirthDate?.ToString("yyyy-MM-dd") ?? oldPatient.BirthDate;

            return oldPatient;
        }
    }
}