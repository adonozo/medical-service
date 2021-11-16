namespace QMUL.DiabetesBackend.MongoDb
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using DataInterfaces;
    using Hl7.Fhir.Model;
    using Microsoft.Extensions.Logging;
    using Model;
    using Models;
    using MongoDB.Bson;
    using MongoDB.Driver;
    using Utils;

    /// <summary>
    /// The Medication Request Dao
    /// </summary>
    public class MedicationRequestDao : MongoDaoBase, IMedicationRequestDao
    {
        private readonly IMongoCollection<MongoMedicationRequest> medicationRequestCollection;
        private readonly ILogger<MedicationRequestDao> logger;
        private const string CollectionName = "medicationRequest";

        public MedicationRequestDao(IDatabaseSettings settings, ILogger<MedicationRequestDao> logger) : base(settings)
        {
            this.logger = logger;
            this.medicationRequestCollection = this.Database.GetCollection<MongoMedicationRequest>(CollectionName);
        }

        /// <inheritdoc />
        public async Task<MedicationRequest> CreateMedicationRequest(MedicationRequest newRequest)
        {
            this.logger.LogDebug("Creating medication request");
            var mongoRequest = newRequest.ToMongoMedicationRequest();
            mongoRequest.CreatedAt = DateTime.UtcNow;
            mongoRequest.DosageInstructions = mongoRequest.DosageInstructions.Select(dose =>
            {
                dose.Id = ObjectId.GenerateNewId().ToString();
                return dose;
            });

            await this.medicationRequestCollection.InsertOneAsync(mongoRequest);
            this.logger.LogDebug("Medication request created with ID {Id}", mongoRequest.Id);
            return await this.GetMedicationRequest(mongoRequest.Id);
        }

        /// <inheritdoc />
        public async Task<MedicationRequest> UpdateMedicationRequest(string id, MedicationRequest actualRequest)
        {
            this.logger.LogDebug("Updating medication request with ID {Id}", id);
            var mongoRequest = actualRequest.ToMongoMedicationRequest();
            var result = await this.medicationRequestCollection.ReplaceOneAsync(request => request.Id == id, mongoRequest);
            if (!result.IsAcknowledged)
            {
                throw new InvalidOperationException($"There was an error updating the Medication Request {id}");
            }

            this.logger.LogDebug("Medication request updated {Id}", id);
            return await this.GetMedicationRequest(id);
        }

        /// <inheritdoc />
        public async Task<MedicationRequest> GetMedicationRequest(string id)
        {
            var cursorResult = await this.medicationRequestCollection.FindAsync(request => request.Id == id);
            var result = cursorResult.FirstOrDefault();
            this.logger.LogDebug("Found medication request: {IdOrNull}", result?.Id);
            return result?.ToMedicationRequest();
        }

        /// <inheritdoc />
        public async Task<List<MedicationRequest>> GetMedicationRequestsByIds(string[] ids)
        {
            var idFilter = Builders<MongoMedicationRequest>.Filter
                .In(item => item.Id, ids);
            var cursor = await this.medicationRequestCollection.FindAsync(idFilter);
            var result = await cursor.ToListAsync();
            return result.Select(mongoMedicationRequest => mongoMedicationRequest.ToMedicationRequest()).ToList();
        }

        /// <inheritdoc />
        public async Task<List<MedicationRequest>> GetMedicationRequestFor(string patientId)
        {
            var result = this.medicationRequestCollection.Find(request =>
                    request.PatientReference.ReferenceId == patientId)
                .Project(mongoRequest => mongoRequest.ToMedicationRequest());
            return await result.ToListAsync();
        }

        /// <inheritdoc />
        public async Task<bool> DeleteMedicationRequest(string id)
        {
            this.logger.LogDebug("Deleting medication request with ID: {Id}", id);
            var result = await this.medicationRequestCollection.DeleteOneAsync(request => request.Id == id);
            return result.IsAcknowledged;
        }

        /// <inheritdoc />
        public async Task<MedicationRequest> GetMedicationRequestForDosage(string patientId, string dosageId)
        {
            var result = this.medicationRequestCollection.Find(request =>
                    request.PatientReference.ReferenceId == patientId
                    && request.DosageInstructions.Any(instruction => instruction.Id == dosageId))
                .Project(mongoRequest => mongoRequest.ToMedicationRequest());
            return await result.FirstOrDefaultAsync();
        }

        /// <inheritdoc />
        public async Task<List<MedicationRequest>> GetActiveMedicationRequests(string patientId)
        {
            var result = this.medicationRequestCollection.Find(request =>
                    request.PatientReference.ReferenceId == patientId
                    && request.Status == MedicationRequest.medicationrequestStatus.Active.ToString()
                    && request.IsInsulin == false)
                .Project(mongoRequest => mongoRequest.ToMedicationRequest());
            return await result.ToListAsync();
        }

        /// <inheritdoc />
        public async Task<List<MedicationRequest>> GetAllActiveMedicationRequests(string patientId)
        {
            var result = this.medicationRequestCollection.Find(request =>
                    request.PatientReference.ReferenceId == patientId
                    && request.Status == MedicationRequest.medicationrequestStatus.Active.ToString())
                .Project(mongoRequest => mongoRequest.ToMedicationRequest());
            return await result.ToListAsync();
        }
    }
}