namespace QMUL.DiabetesBackend.MongoDb
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using DataInterfaces;
    using Model;
    using Model.Enums;
    using Models;
    using MongoDB.Bson;
    using MongoDB.Driver;
    using Utils;

    /// <summary>
    /// The Patient Dao.
    /// </summary>
    public class PatientDao : MongoDaoBase, IPatientDao
    {
        private readonly IMongoCollection<MongoPatient> patientCollection;
        private const string CollectionName = "patient";
        
        public PatientDao(IDatabaseSettings settings) : base(settings)
        {
            this.patientCollection = this.Database.GetCollection<MongoPatient>(CollectionName);
        }

        /// <inheritdoc />
        public async Task<List<Patient>> GetPatients()
        {
            var result = this.patientCollection.Find(FilterDefinition<MongoPatient>.Empty)
                .Project(patient => patient.ToPatient());
            return await result.ToListAsync();
        }

        /// <inheritdoc />
        public async Task<Patient> CreatePatient(Patient newPatient)
        {
            newPatient.ExactEventTimes ??= new Dictionary<CustomEventTiming, DateTime>();
            newPatient.ResourceStartDate ??= new Dictionary<string, DateTime>();
            var mongoPatient = newPatient.ToMongoPatient();
            await this.patientCollection.InsertOneAsync(mongoPatient);
            return await this.GetPatientByIdOrEmail(mongoPatient.Id);
        }

        /// <inheritdoc />
        public async Task<Patient> GetPatientByIdOrEmail(string idOrEmail)
        {
            IFindFluent<MongoPatient, Patient> result;
            if (ObjectId.TryParse(idOrEmail, out var _))
            {
                result = this.patientCollection.Find(patient => patient.Id == idOrEmail)
                    .Project(patient => patient.ToPatient());                
            }
            else
            {
                result = this.patientCollection.Find(patient => patient.Email == idOrEmail)
                    .Project(patient => patient.ToPatient());
            }
            
            return await result.FirstOrDefaultAsync();
        }

        /// <inheritdoc />
        public async Task<bool> UpdatePatient(Patient actualPatient)
        {
            var mongoPatient = actualPatient.ToMongoPatient();
            var result = await this.patientCollection.ReplaceOneAsync(patient => patient.Id == actualPatient.Id,
                mongoPatient, new ReplaceOptions { IsUpsert = true });
            return result.IsAcknowledged;
        }
    }
}