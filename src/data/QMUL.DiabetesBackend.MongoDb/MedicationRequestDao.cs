namespace QMUL.DiabetesBackend.MongoDb
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using DataInterfaces;
    using DataInterfaces.Exceptions;
    using Hl7.Fhir.Model;
    using Hl7.Fhir.Utility;
    using Microsoft.Extensions.Logging;
    using Model.Extensions;
    using MongoDB.Bson;
    using MongoDB.Driver;
    using Utils;
    using Task = System.Threading.Tasks.Task;

    /// <summary>
    /// The Medication Request Dao
    /// </summary>
    public class MedicationRequestDao : MongoDaoBase, IMedicationRequestDao
    {
        private const string CollectionName = "medicationRequest";

        private readonly IMongoCollection<BsonDocument> medicationRequestCollection;
        private readonly ILogger<MedicationRequestDao> logger;

        public MedicationRequestDao(IMongoDatabase database, ILogger<MedicationRequestDao> logger) : base(database)
        {
            this.logger = logger;
            this.medicationRequestCollection = this.Database.GetCollection<BsonDocument>(CollectionName);
        }

        /// <inheritdoc />
        public async Task<MedicationRequest> CreateMedicationRequest(MedicationRequest newRequest)
        {
            this.logger.LogDebug("Creating medication request");
            SetDosageId(newRequest, true);

            var document = await RequestToBsonDocument(newRequest);
            await this.medicationRequestCollection.InsertOneAsync(document);
            var newId = document["_id"].ToString();

            this.logger.LogDebug("Medication request created with ID {Id}", newId);
            var errorMessage = $"The medication request was not created";
            return await this.GetSingleMedicationRequestOrThrow(newId, new CreateException(errorMessage));
        }

        /// <inheritdoc />
        public async Task<MedicationRequest> UpdateMedicationRequest(string id, MedicationRequest actualRequest)
        {
            this.logger.LogDebug("Updating medication request with ID {Id}", id);
            SetDosageId(actualRequest);

            var document =  await RequestToBsonDocument(actualRequest);
            var result = await this.medicationRequestCollection
                .ReplaceOneAsync(Helpers.GetByIdFilter(id), document);

            var errorMessage = $"There was an error updating the Medication Request {id}";
            this.CheckAcknowledgedOrThrow(result.IsAcknowledged, new UpdateException(errorMessage),
                () => this.logger.LogWarning("{ErrorMessage}", errorMessage));
            this.logger.LogDebug("Medication request updated {Id}", id);
            return await this.GetSingleMedicationRequestOrThrow(id, new UpdateException(errorMessage));
        }

        /// <inheritdoc />
        public async Task<MedicationRequest> GetMedicationRequest(string id)
        {
            var errorMessage = $"Could not find medication request with ID {id}";
            var result = await this.GetSingleMedicationRequestOrThrow(id, new NotFoundException(errorMessage));
            return result;
        }

        /// <inheritdoc />
        public async Task<IList<MedicationRequest>> GetMedicationRequestsByIds(string[] ids)
        {
            var idFilter = Builders<BsonDocument>.Filter
                .In("_id", ids);
            var results = await this.medicationRequestCollection.Find(idFilter)
                .Project(document => Helpers.ToResourceAsync<MedicationRequest>(document))
                .ToListAsync();
            return await Task.WhenAll(results);
        }

        /// <inheritdoc />
        public async Task<IList<MedicationRequest>> GetMedicationRequestFor(string patientId)
        {
            var filter = Helpers.GetPatientReferenceFilter(patientId);
            var results = await this.medicationRequestCollection.Find(filter)
                .Project(document => Helpers.ToResourceAsync<MedicationRequest>(document))
                .ToListAsync();
            return await Task.WhenAll(results);
        }

        /// <inheritdoc />
        public async Task<bool> DeleteMedicationRequest(string id)
        {
            this.logger.LogDebug("Deleting medication request with ID: {Id}", id);
            var result = await this.medicationRequestCollection.DeleteOneAsync(Helpers.GetByIdFilter(id));
            return result.IsAcknowledged;
        }

        /// <inheritdoc />
        public async Task<MedicationRequest> GetMedicationRequestForDosage(string patientId, string dosageId)
        {
            var filters = Builders<BsonDocument>.Filter.And(
                Helpers.GetPatientReferenceFilter(patientId),
                Builders<BsonDocument>.Filter.Eq("dosageInstruction.elementId", dosageId));
            var cursor = this.medicationRequestCollection.Find(filters);
            var errorMessage = $"Could not found the medication request for the dosage ID {dosageId}";
            var document = await this.GetSingleOrThrow(cursor, new NotFoundException(errorMessage));
            return await Helpers.ToResourceAsync<MedicationRequest>(document);
        }

        /// <inheritdoc />
        public async Task<IList<MedicationRequest>> GetActiveMedicationRequests(string patientId)
        {
            var filters = Builders<BsonDocument>.Filter.And(
                Helpers.GetPatientReferenceFilter(patientId),
                Builders<BsonDocument>.Filter.Eq("isInsulin", false),
                Builders<BsonDocument>.Filter.Eq("status",
                    MedicationRequest.medicationrequestStatus.Active.GetLiteral()));

            var results = await this.medicationRequestCollection.Find(filters)
                .Project(document => Helpers.ToResourceAsync<MedicationRequest>(document))
                .ToListAsync();
            return await Task.WhenAll(results);
        }

        /// <inheritdoc />
        public async Task<IList<MedicationRequest>> GetAllActiveMedicationRequests(string patientId)
        {
            var filters = Builders<BsonDocument>.Filter.And(
                Helpers.GetPatientReferenceFilter(patientId),
                Builders<BsonDocument>.Filter.Eq("status",
                    MedicationRequest.medicationrequestStatus.Active.GetLiteral()));

            var results = await this.medicationRequestCollection.Find(filters)
                .Project(document => Helpers.ToResourceAsync<MedicationRequest>(document))
                .ToListAsync();
            return await Task.WhenAll(results);
        }

        private async Task<MedicationRequest> GetSingleMedicationRequestOrThrow(string id, DataExceptionBase exception,
            Action fallback = null)
        {
            var cursor = this.medicationRequestCollection.Find(Helpers.GetByIdFilter(id));
            var document = await this.GetSingleOrThrow(cursor, exception, fallback);
            return await Helpers.ToResourceAsync<MedicationRequest>(document);
        }

        private static void SetDosageId(MedicationRequest request, bool force = false)
        {
            var emptyDosageIds = request.DosageInstruction
                .Where(dosage => force || string.IsNullOrWhiteSpace(dosage.ElementId));
            foreach (var dosage in emptyDosageIds)
            {
                dosage.ElementId = ObjectId.GenerateNewId().ToString();
            }
        }

        private static async Task<BsonDocument> RequestToBsonDocument(MedicationRequest request)
        {
            var document =  await Helpers.ToBsonDocumentAsync(request);
            document["isInsulin"] = request.HasInsulinFlag();
            return document;
        }
    }
}