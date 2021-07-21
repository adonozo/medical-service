using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using MongoDB.Driver;
using QMUL.DiabetesBackend.DataInterfaces;
using QMUL.DiabetesBackend.Model;
using QMUL.DiabetesBackend.MongoDb.Models;

namespace QMUL.DiabetesBackend.MongoDb
{
    public class MedicationRequestDao : BaseMongoDao, IMedicationRequestDao
    {
        private readonly IMongoCollection<MongoMedicationRequest> medicationRequestCollection;
        private const string CollectionName = "MedicationRequest";
        
        public MedicationRequestDao(IDatabaseSettings settings) : base(settings)
        {
            this.medicationRequestCollection = this.Database.GetCollection<MongoMedicationRequest>(CollectionName);
        }

        public Task<MedicationRequest> CreateMedicationRequest(MedicationRequest newRequest)
        {
            throw new NotImplementedException();
        }

        public Task<MedicationRequest> UpdateMedicationRequest(string id, MedicationRequest actualRequest)
        {
            throw new NotImplementedException();
        }

        public Task<MedicationRequest> GetMedicationRequest(string id)
        {
            throw new NotImplementedException();
        }

        public Task<List<MedicationRequest>> GetMedicationRequestFor(string patientId, DateTime dateTime, int intervalMin = 10)
        {
            throw new NotImplementedException();
        }

        public Task<List<MedicationRequest>> GetMedicationRequestFor(string patientId, DateTime dateTime, Timing.EventTiming timing)
        {
            throw new NotImplementedException();
        }

        public Task<List<MedicationRequest>> GetMedicationRequestFor(string patientId, DateTime startTime, DateTime endTime)
        {
            throw new NotImplementedException();
        }

        public Task<List<MedicationRequest>> GetNextMedicationRequestFor(string patientId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteMedicationRequest(string id)
        {
            throw new NotImplementedException();
        }
    }
}