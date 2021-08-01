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

        public async Task<List<MedicationRequest>> GetMedicationRequestFor(string patientId, DateTime dateTime, int intervalMin)
        {
            var startRange = dateTime.AddMinutes(intervalMin);
            var endRange = dateTime.AddMinutes(intervalMin);
            var timeCompare = new Func<string, bool>(time =>
            {
                var dateCompare = DateTime.Parse($"{dateTime:yyyy-MM-dd}T{time}");
                return dateCompare > startRange && dateCompare < endRange;
            });
            var cursor =
                await this.medicationRequestCollection.FindAsync(request =>
                    request.PatientReference.ReferenceId == patientId &&
                    request.DosageInstructions.Any(instruction =>
                        instruction.Timing.TimesOfDay.Any(time => timeCompare(time))));
            var requestList = await cursor.ToListAsync();
            
            // TODO still need to consider day of week
            return requestList.Select(mongo => mongo.ToMedicationRequest()).ToList();
        }

        public Task<List<MedicationRequest>> GetMedicationRequestFor(string patientId, DateTime dateTime, Timing.EventTiming timing)
        {
            throw new NotImplementedException();
        }

        public async Task<List<MedicationRequest>> GetMedicationRequestFor(string patientId, DateTime dateTime)
        {
            var timeCompare = new Func<MongoTiming, bool>(timing =>
                dateTime > timing.PeriodStart && dateTime < timing.PeriodEnd);
            var cursor =
                await this.medicationRequestCollection.FindAsync(request =>
                    request.PatientReference.ReferenceId == patientId &&
                    request.DosageInstructions.Any(instruction => timeCompare(instruction.Timing)));
            var requestList = await cursor.ToListAsync();
            
            // TODO still need to consider day of week
            return requestList.Select(mongo => mongo.ToMedicationRequest()).ToList();
        }

        public Task<List<MedicationRequest>> GetNextMedicationRequestFor(string patientId)
        {
            throw new NotImplementedException();
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
                .Project(mongoPatient => mongoPatient.ToMedicationRequest());
            return await result.FirstOrDefaultAsync();
        }

        public async Task<List<MedicationRequest>> GetActiveMedicationRequests(string patientId)
        {
            var result = this.medicationRequestCollection.Find(request =>
                    request.PatientReference.ReferenceId == patientId
                    && request.Status == MedicationRequest.medicationrequestStatus.Active.ToString()
                    && request.IsInsulin == false)
                .Project(mongoPatient => mongoPatient.ToMedicationRequest());
            return await result.ToListAsync();
        }
    }
}