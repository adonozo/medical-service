using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using QMUL.DiabetesBackend.DataInterfaces;
using QMUL.DiabetesBackend.Model;
using QMUL.DiabetesBackend.MongoDb.Models;
using QMUL.DiabetesBackend.MongoDb.Utils;

namespace QMUL.DiabetesBackend.MongoDb
{
    public class PatientDao : MongoDaoBase, IPatientDao
    {
        private readonly IMongoCollection<MongoPatient> patientCollection;
        private const string CollectionName = "patient";
        
        public PatientDao(IDatabaseSettings settings) : base(settings)
        {
            this.patientCollection = this.Database.GetCollection<MongoPatient>(CollectionName);
        }

        public async Task<List<Patient>> GetPatients()
        {
            var result = this.patientCollection.Find(FilterDefinition<MongoPatient>.Empty)
                .Project(patient => patient.ToPatient());
            return await result.ToListAsync();
        }

        public async Task<Patient> CreatePatient(Patient newPatient)
        {
            await this.patientCollection.InsertOneAsync(newPatient.ToMongoPatient());
            return await this.GetPatientByIdOrEmail(newPatient.Id);
        }

        public async Task<Patient> GetPatientByIdOrEmail(string idOrEmail)
        {
            var result = this.patientCollection.Find(patient => patient.Id == idOrEmail || patient.Email == idOrEmail)
                .Project(patient => patient.ToPatient());
            return await result.FirstOrDefaultAsync();
        }

        public async Task<bool> UpdatePatient(Patient actualPatient)
        {
            var result = await this.patientCollection.ReplaceOneAsync(patient => patient.Id == actualPatient.Id,
                actualPatient.ToMongoPatient());
            return result.IsAcknowledged;
        }
    }
}