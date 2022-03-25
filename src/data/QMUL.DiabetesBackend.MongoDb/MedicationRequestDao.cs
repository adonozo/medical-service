namespace QMUL.DiabetesBackend.MongoDb
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using DataInterfaces;
    using DataInterfaces.Exceptions;
    using Hl7.Fhir.Model;
    using Microsoft.Extensions.Logging;
    using Model.Constants;
    using Model.Extensions;
    using MongoDB.Bson;
    using MongoDB.Driver;

    /// <summary>
    /// The Medication Request Dao
    /// </summary>
    public class MedicationRequestDao : MongoDaoBase, IMedicationRequestDao
    {
        private readonly IMongoCollection<MedicationRequest> medicationRequestCollection;
        private readonly ILogger<MedicationRequestDao> logger;
        private const string CollectionName = "medicationRequest";

        public MedicationRequestDao(IMongoDatabase database, ILogger<MedicationRequestDao> logger) : base(database)
        {
            this.logger = logger;
            this.medicationRequestCollection = this.Database.GetCollection<MedicationRequest>(CollectionName);
        }

        /// <inheritdoc />
        public async Task<MedicationRequest> CreateMedicationRequest(MedicationRequest newRequest)
        {
            this.logger.LogDebug("Creating medication request");
            foreach (var dosage in newRequest.DosageInstruction)
            {
                dosage.ElementId = ObjectId.GenerateNewId().ToString();
            }

            newRequest.Id = ObjectId.GenerateNewId().ToString();
            await this.medicationRequestCollection.InsertOneAsync(newRequest);
            this.logger.LogDebug("Medication request created with ID {Id}", newRequest.Id);
            var errorMessage = $"The medication request was not created";
            return await this.GetSingleMedicationRequestOrThrow(newRequest.Id, new CreateException(errorMessage),
                () => { this.logger.LogWarning("{ErrorMessage}", errorMessage); });
        }

        /// <inheritdoc />
        public async Task<MedicationRequest> UpdateMedicationRequest(string id, MedicationRequest actualRequest)
        {
            this.logger.LogDebug("Updating medication request with ID {Id}", id);
            var result =
                await this.medicationRequestCollection.ReplaceOneAsync(request => request.Id == id, actualRequest);
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
            var idFilter = Builders<MedicationRequest>.Filter
                .In(item => item.Id, ids);
            var result = this.medicationRequestCollection.Find(idFilter);
            return await result.ToListAsync();
        }

        /// <inheritdoc />
        public async Task<List<MedicationRequest>> GetMedicationRequestFor(string patientId)
        {
            var patientReference = Constants.PatientPath + patientId;
            var result = this.medicationRequestCollection.Find(request =>
                    request.Subject.Reference == patientReference);
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
            var patientReference = Constants.PatientPath + patientId;
            var result = this.medicationRequestCollection.Find(request =>
                    request.Subject.Reference == patientReference
                    && request.DosageInstruction.Any(instruction => instruction.ElementId == dosageId));
            var errorMessage = $"Could not found the medication request for the dosage ID {dosageId}";
            return await this.GetSingleOrThrow(result, new NotFoundException(errorMessage),
                () => this.logger.LogWarning("{ErrorMessage}", errorMessage));
        }

        /// <inheritdoc />
        public async Task<List<MedicationRequest>> GetActiveMedicationRequests(string patientId)
        {
            var patientReference = Constants.PatientPath + patientId;
            var result = this.medicationRequestCollection.Find(request =>
                    request.Subject.Reference == patientReference
                    && request.Status == MedicationRequest.medicationrequestStatus.Active
                    && request.HasInsulinFlag() == false);
            return await result.ToListAsync();
        }

        /// <inheritdoc />
        public async Task<List<MedicationRequest>> GetAllActiveMedicationRequests(string patientId)
        {
            var patientReference = Constants.PatientPath + patientId;
            var result = this.medicationRequestCollection.Find(request =>
                    request.Subject.Reference == patientReference
                    && request.Status == MedicationRequest.medicationrequestStatus.Active);
            return await result.ToListAsync();
        }

        private async Task<MedicationRequest> GetSingleMedicationRequestOrThrow(string id, DataExceptionBase exception,
            Action fallback)
        {
            var result = this.medicationRequestCollection.Find(request => request.Id == id);
            return await this.GetSingleOrThrow(result, exception, fallback);
        }
    }
}