namespace QMUL.DiabetesBackend.MongoDb
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using DataInterfaces;
    using Microsoft.Extensions.Logging;
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
        private readonly ILogger<PatientDao> logger;
        
        public PatientDao(IDatabaseSettings settings, ILogger<PatientDao> logger) : base(settings)
        {
            this.logger = logger;
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
            this.logger.LogInformation("Creating patient {FirstName} {LastName}", newPatient.FirstName, newPatient.LastName);
            newPatient.ExactEventTimes ??= new Dictionary<CustomEventTiming, DateTime>();
            newPatient.ResourceStartDate ??= new Dictionary<string, DateTime>();
            var mongoPatient = newPatient.ToMongoPatient();
            await this.patientCollection.InsertOneAsync(mongoPatient);
            this.logger.LogInformation("Patient {FirstName} {LastName} created with ID: {Id}", mongoPatient.FirstName,
                mongoPatient.LastName, mongoPatient.Id);
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
            logger.LogInformation("Updating patient with ID: {Id}", actualPatient.Id);
            var mongoPatient = actualPatient.ToMongoPatient();
            var result = await this.patientCollection.ReplaceOneAsync(patient => patient.Id == actualPatient.Id,
                mongoPatient, new ReplaceOptions { IsUpsert = true });
            logger.LogInformation("Patient with ID {Id} updated", actualPatient.Id);
            return result.IsAcknowledged;
        }
    }
}