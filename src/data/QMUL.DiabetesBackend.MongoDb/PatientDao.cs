namespace QMUL.DiabetesBackend.MongoDb
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AutoMapper;
    using DataInterfaces;
    using DataInterfaces.Exceptions;
    using Hl7.Fhir.Model;
    using Microsoft.Extensions.Logging;
    using Model;
    using Model.Constants;
    using Model.Extensions;
    using Models;
    using MongoDB.Bson;
    using MongoDB.Driver;

    /// <summary>
    /// The Patient Dao.
    /// </summary>
    public class PatientDao : MongoDaoBase, IPatientDao
    {
        private readonly IMongoCollection<MongoPatient> patientCollection;
        private const string CollectionName = "patient";
        private readonly ILogger<PatientDao> logger;
        private readonly IMapper mapper;

        public PatientDao(IMongoDatabase database, IMapper mapper, ILogger<PatientDao> logger) : base(database)
        {
            this.logger = logger;
            this.mapper = mapper;
            this.patientCollection = this.Database.GetCollection<MongoPatient>(CollectionName);
        }

        /// <inheritdoc />
        public async Task<List<Patient>> GetPatients()
        {
            var result = this.patientCollection.Find(FilterDefinition<MongoPatient>.Empty)
                .Project(patient => mapper.Map<Patient>(patient));
            return await result.ToListAsync();
        }

        /// <inheritdoc />
        public async Task<Patient> CreatePatient(Patient newPatient)
        {
            this.logger.LogInformation("Inserting patient...");
            //newPatient.ExactEventTimes ??= new Dictionary<CustomEventTiming, DateTimeOffset>();
            //newPatient.ResourceStartDate ??= new Dictionary<string, DateTime>();
            var mongoPatient = mapper.Map<MongoPatient>(newPatient);
            await this.patientCollection.InsertOneAsync(mongoPatient);
            this.logger.LogInformation("Patient created with ID: {Id}", mongoPatient.Id);
            return await this.GetSinglePatientOrThrow(mongoPatient.Id);
        }

        /// <inheritdoc />
        public async Task<Patient> GetPatientByIdOrEmail(string idOrEmail)
        {
            IFindFluent<MongoPatient, Patient> result;
            if (ObjectId.TryParse(idOrEmail, out _))
            {
                result = this.patientCollection.Find(patient => patient.Id == idOrEmail)
                    .Project(patient => mapper.Map<Patient>(patient));
            }
            else
            {
                result = this.patientCollection.Find(patient => patient.Email == idOrEmail)
                    .Project(patient => mapper.Map<Patient>(patient));
            }

            var errorMessage = $"Could not find patient with ID or email {idOrEmail}";
            return await this.GetSingleOrThrow(result, new NotFoundException(errorMessage),
                () => this.logger.LogInformation("{ErrorMessage}", errorMessage));
        }

        /// <inheritdoc />
        public async Task<Patient> UpdatePatient(Patient actualPatient)
        {
            logger.LogInformation("Updating patient with ID: {Id}", actualPatient.Id);
            var mongoPatient = mapper.Map<MongoPatient>(actualPatient);
            var result = await this.patientCollection.ReplaceOneAsync(patient => patient.Id == actualPatient.Id,
                mongoPatient, new ReplaceOptions {IsUpsert = true});
            logger.LogInformation("Patient with ID {Id} updated", actualPatient.Id);
            var errorMessage = $"Could not update patient with ID {actualPatient.Id}";
            this.CheckAcknowledgedOrThrow(result.IsAcknowledged, new UpdateException(errorMessage),
                () => this.logger.LogWarning("{ErrorMessage}", errorMessage));
            return await this.GetSinglePatientOrThrow(actualPatient.Id);
        }

        /// <inheritdoc />
        public async Task<Patient> PatchPatient(InternalPatient actualPatient, Patient oldPatient)
        {
            logger.LogInformation("Updating patient with ID: {Id}", actualPatient.Id);
            var updatedPatient = this.SetPatientData(actualPatient, oldPatient);
            return await UpdatePatient(updatedPatient);
        }

        private Patient SetPatientData(InternalPatient patient, Patient oldPatient)
        {
            if (!string.IsNullOrEmpty(patient.LastName))
            {
                oldPatient.Name[0].Family = patient.LastName;
            }
            if (!string.IsNullOrEmpty(patient.FirstName))
            {
                oldPatient.Name[0].Given = new[] {patient.FirstName};
            }
            if (!string.IsNullOrEmpty(patient.Email))
            {
                oldPatient.SetEmailExtension(patient.Email);
            }
            if (!string.IsNullOrEmpty(patient.AlexaUserId))
            {
                oldPatient.SetAlexaIdExtension(patient.AlexaUserId);
            }
            
            oldPatient.Gender = patient.Gender ?? oldPatient.Gender;
            oldPatient.BirthDate = patient.BirthDate?.ToString("O") ?? oldPatient.BirthDate;
            oldPatient.Contact = patient.PhoneContacts as List<Patient.ContactComponent> ?? oldPatient.Contact;
            
            return oldPatient;
        }

        private async Task<Patient> GetSinglePatientOrThrow(string id)
        {
            var result = this.patientCollection.Find(patient => patient.Id == id)
                .Project(patient => mapper.Map<Patient>(patient));
            const string errorMessage = "Could not create patient";
            return await this.GetSingleOrThrow(result, new CreateException(errorMessage),
                () => this.logger.LogWarning("{ErrorMessage}", errorMessage));
        }
    }
}