using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using MongoDB.Bson;
using MongoDB.Driver;
using QMUL.DiabetesBackend.DataInterfaces;
using QMUL.DiabetesBackend.Model;
using QMUL.DiabetesBackend.MongoDb.Models;
using QMUL.DiabetesBackend.MongoDb.Utils;

namespace QMUL.DiabetesBackend.MongoDb
{
    public class MedicationRequestDao : MongoDaoBase, IMedicationRequestDao
    {
        private readonly IMongoCollection<MongoMedicationRequest> medicationRequestCollection;
        private const string CollectionName = "medicationRequest";
        
        public MedicationRequestDao(IDatabaseSettings settings) : base(settings)
        {
            this.medicationRequestCollection = this.Database.GetCollection<MongoMedicationRequest>(CollectionName);
        }

        public async Task<MedicationRequest> CreateMedicationRequest(MedicationRequest newRequest)
        {
            var mongoRequest = newRequest.ToMongoMedicationRequest();
            mongoRequest.CreatedAt = DateTime.UtcNow;
            mongoRequest.DosageInstructions = mongoRequest.DosageInstructions.Select(dose =>
            {
                dose.Id = ObjectId.GenerateNewId().ToString();
                return dose;
            });
            
            await this.medicationRequestCollection.InsertOneAsync(mongoRequest);
            return await this.GetMedicationRequest(mongoRequest.Id);
        }

        public async Task<MedicationRequest> UpdateMedicationRequest(string id, MedicationRequest actualRequest)
        {
            var mongoRequest = actualRequest.ToMongoMedicationRequest();
            var result = await this.medicationRequestCollection.ReplaceOneAsync(request => request.Id == id, mongoRequest);
            if (result.IsAcknowledged)
            {
                return await this.GetMedicationRequest(id);
            }

            throw new InvalidOperationException($"there was an error updating the Medication Request {id}");
        }

        public async Task<MedicationRequest> GetMedicationRequest(string id)
        {
            var cursorResult = await this.medicationRequestCollection.FindAsync(request => request.Id == id);
            var result = cursorResult.FirstOrDefault();
            return result?.ToMedicationRequest();
        }

        public async Task<List<MedicationRequest>> GetMedicationRequestsByIds(string[] ids)
        {
            var idFilter = Builders<MongoMedicationRequest>.Filter
                .In(item => item.Id, ids);
            var cursor = await this.medicationRequestCollection.FindAsync(idFilter);
            var result = await cursor.ToListAsync();
            return result.Select(mongoMedicationRequest => mongoMedicationRequest.ToMedicationRequest()).ToList();
        }

        public async Task<List<MedicationRequest>> GetMedicationRequestFor(string patientId)
        {
            var result = this.medicationRequestCollection.Find(request =>
                    request.PatientReference.ReferenceId == patientId)
                .Project(mongoRequest => mongoRequest.ToMedicationRequest());
            return await result.ToListAsync();
        }

        public async Task<bool> DeleteMedicationRequest(string id)
        {
            var result = await this.medicationRequestCollection.DeleteOneAsync(request => request.Id == id);
            return result.IsAcknowledged;
        }

        public async Task<MedicationRequest> GetMedicationRequestForDosage(string patientId, string dosageId)
        {
            var result = this.medicationRequestCollection.Find(request =>
                    request.PatientReference.ReferenceId == patientId
                    && request.DosageInstructions.Any(instruction => instruction.Id == dosageId))
                .Project(mongoRequest => mongoRequest.ToMedicationRequest());
            return await result.FirstOrDefaultAsync();
        }

        public async Task<List<MedicationRequest>> GetActiveMedicationRequests(string patientId)
        {
            var result = this.medicationRequestCollection.Find(request =>
                    request.PatientReference.ReferenceId == patientId
                    && request.Status == MedicationRequest.medicationrequestStatus.Active.ToString()
                    && request.IsInsulin == false)
                .Project(mongoRequest => mongoRequest.ToMedicationRequest());
            return await result.ToListAsync();
        }

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