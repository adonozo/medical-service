﻿namespace QMUL.DiabetesBackend.MongoDb
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using DataInterfaces;
    using DataInterfaces.Exceptions;
    using Hl7.Fhir.Model;
    using Microsoft.Extensions.Logging;
    using Models;
    using MongoDB.Bson;
    using MongoDB.Driver;
    using Utils;

    /// <summary>
    /// The Medication Request Dao
    /// </summary>
    public class MedicationRequestDao : MongoDaoBase, IMedicationRequestDao
    {
        // TODO stop using MongoMedicationRequest
        private readonly IMongoCollection<MongoMedicationRequest> medicationRequestCollection;
        private readonly ILogger<MedicationRequestDao> logger;
        private const string CollectionName = "medicationRequest";

        public MedicationRequestDao(IMongoDatabase database, ILogger<MedicationRequestDao> logger) : base(database)
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
            var errorMessage = $"The medication request was not created";
            return await this.GetSingleMedicationRequestOrThrow(mongoRequest.Id, new CreateException(errorMessage),
                () => { this.logger.LogWarning("{ErrorMessage}", errorMessage); });
        }

        /// <inheritdoc />
        public async Task<MedicationRequest> UpdateMedicationRequest(string id, MedicationRequest actualRequest)
        {
            this.logger.LogDebug("Updating medication request with ID {Id}", id);
            var mongoRequest = actualRequest.ToMongoMedicationRequest();
            var result =
                await this.medicationRequestCollection.ReplaceOneAsync(request => request.Id == id, mongoRequest);
            var errorMessage = $"There was an error updating the Medication Request {id}";
            this.CheckAcknowledgedOrThrow(result.IsAcknowledged, new UpdateException(errorMessage),
                () => this.logger.LogWarning("{ErrorMessage}", errorMessage));
            this.logger.LogDebug("Medication request updated {Id}", id);
            return await this.GetSingleMedicationRequestOrThrow(id, new UpdateException(errorMessage),
                () => this.logger.LogWarning("{ErrorMessage}", errorMessage));
        }

        /// <inheritdoc />
        public async Task<MedicationRequest> GetMedicationRequest(string id)
        {
            var errorMessage = $"Could not find medication request with ID {id}";
            var result = await this.GetSingleMedicationRequestOrThrow(id, new NotFoundException(errorMessage),
                () => this.logger.LogWarning("{ErrorMessage}", errorMessage));
            this.logger.LogTrace("Medication Request with {Id} found", id);
            return result;
        }

        /// <inheritdoc />
        public async Task<List<MedicationRequest>> GetMedicationRequestsByIds(string[] ids)
        {
            var idFilter = Builders<MongoMedicationRequest>.Filter
                .In(item => item.Id, ids);
            var result = this.medicationRequestCollection.Find(idFilter)
                .Project(mongoMedicationRequest => mongoMedicationRequest.ToMedicationRequest());
            return await result.ToListAsync();
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
            var errorMessage = $"Could not found the medication request for the dosage ID {dosageId}";
            return await this.GetSingleOrThrow(result, new NotFoundException(errorMessage),
                () => this.logger.LogWarning("{ErrorMessage}", errorMessage));
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

        private async Task<MedicationRequest> GetSingleMedicationRequestOrThrow(string id, DataExceptionBase exception,
            Action fallback)
        {
            var result = this.medicationRequestCollection.Find(request => request.Id == id)
                .Project(medicationRequest => medicationRequest.ToMedicationRequest());
            return await this.GetSingleOrThrow(result, exception, fallback);
        }
    }
}