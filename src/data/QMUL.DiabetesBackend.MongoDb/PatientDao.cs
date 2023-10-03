namespace QMUL.DiabetesBackend.MongoDb;

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using DataInterfaces;
using Hl7.Fhir.Model;
using Microsoft.Extensions.Logging;
using Model;
using Model.Exceptions;
using Model.Extensions;
using Model.Utils;
using MongoDB.Bson;
using MongoDB.Driver;
using Utils;
using Task = System.Threading.Tasks.Task;

/// <summary>
/// The Patient Dao.
/// </summary>
public class PatientDao : MongoDaoBase, IPatientDao
{
    private const string CollectionName = "patient";

    private readonly ILogger<PatientDao> logger;
    private readonly IMongoCollection<BsonDocument> patientCollection;

    public PatientDao(IMongoDatabase database, ILogger<PatientDao> logger) : base(database)
    {
        this.logger = logger;
        this.Database.GetCollection<Patient>(CollectionName);
        this.patientCollection = this.Database.GetCollection<BsonDocument>(CollectionName);
    }

    /// <inheritdoc />
    public async Task<PaginatedResult<IEnumerable<Resource>>> GetPatients(PaginationRequest paginationRequest)
    {
        var searchFilter = FilterDefinition<BsonDocument>.Empty;
        var resultsFilter = Helpers.GetPaginationFilter(searchFilter, paginationRequest.LastCursorId);
        var documents = await this.patientCollection.Find(resultsFilter)
            .Sort(Helpers.GetDefaultOrder())
            .Limit(paginationRequest.Limit)
            .ToListAsync();
        var results = documents.Select(Helpers.ToResourceAsync<Patient>);
        Resource[] patients = await Task.WhenAll(results);
        if (patients.Length == 0)
        {
            return new PaginatedResult<IEnumerable<Resource>> { Results = patients };
        }

        return await Helpers.GetPaginatedResult(patientCollection, searchFilter, patients);
    }

    /// <inheritdoc />
    public async Task<Patient> CreatePatient(Patient newPatient)
    {
        this.logger.LogInformation("Inserting patient...");
        var document = await this.PatientToBsonDocument(newPatient);
        await this.patientCollection.InsertOneAsync(document);

        var newId = this.GetIdFromDocument(document);
        this.logger.LogInformation("Patient created with ID: {Id}", newId);
        return await this.GetSinglePatientOrThrow(newId);
    }

    /// <inheritdoc />
    public async Task<Patient?> GetPatientByIdOrEmail(string idOrEmail)
    {
        var filter = ObjectId.TryParse(idOrEmail, out _)
            ? Helpers.GetByIdFilter(idOrEmail)
            : Builders<BsonDocument>.Filter.Eq("email", idOrEmail);
        var bsonPatient = await this.patientCollection.Find(filter).FirstOrDefaultAsync();
        if (bsonPatient is null)
        {
            return null;
        }

        return await Helpers.ToResourceAsync<Patient>(bsonPatient);
    }

    /// <inheritdoc />
    public async Task<bool> UpdatePatient(Patient actualPatient)
    {
        logger.LogInformation("Updating patient with ID: {Id}", actualPatient.Id);
        var bson = await this.PatientToBsonDocument(actualPatient);
        var result = await this.patientCollection.ReplaceOneAsync(Helpers.GetByIdFilter(actualPatient.Id),
            bson, new ReplaceOptions { IsUpsert = true });

        return result.IsAcknowledged;
    }

    /// <inheritdoc />
    public async Task<bool> PatchPatient(InternalPatient actualPatient, Patient oldPatient)
    {
        logger.LogInformation("Updating patient with ID: {Id}", actualPatient.Id);
        var updatedPatient = await this.SetPatientData(actualPatient, oldPatient);
        return await this.UpdatePatient(updatedPatient);
    }

    private async Task<Patient> GetSinglePatientOrThrow(string id)
    {
        var cursor = this.patientCollection.Find(Helpers.GetByIdFilter(id));
        const string errorMessage = "Could not create or update the patient";
        var bsonDocument = await this.GetSingleOrThrow(cursor, new WriteResourceException(errorMessage),
            () => this.logger.LogWarning("{ErrorMessage}", errorMessage));
        return await Helpers.ToResourceAsync<Patient>(bsonDocument);
    }

    private async Task<BsonDocument> PatientToBsonDocument(Patient patient)
    {
        var bson = await Helpers.ToBsonDocumentAsync(patient);
        var email = patient.GetEmailExtension();
        if (string.IsNullOrEmpty(email))
        {
            return bson;
        }

        bson.Add("email", email);
        return bson;
    }

    private async Task<Patient> SetPatientData(InternalPatient patient, Patient oldPatient)
    {
        if (!string.IsNullOrEmpty(patient.LastName))
        {
            oldPatient.Name[0].Family = patient.LastName;
        }

        if (!string.IsNullOrEmpty(patient.FirstName))
        {
            oldPatient.Name[0].Given = new[] { patient.FirstName };
        }

        if (!string.IsNullOrEmpty(patient.Email))
        {
            oldPatient.SetEmailExtension(patient.Email);
        }

        if (!string.IsNullOrEmpty(patient.AlexaUserId))
        {
            oldPatient.SetAlexaIdExtension(patient.AlexaUserId);
        }

        if (patient.Phones != null)
        {
            var phonesTask = patient.Phones.Select(Converter.ToDataTypeAsync<ContactPoint>);
            var phones = await Task.WhenAll(phonesTask);
            oldPatient.Telecom = phones.ToList();
        }

        oldPatient.Gender = patient.Gender ?? oldPatient.Gender;
        oldPatient.BirthDate = patient.BirthDate?.ToString("R", CultureInfo.InvariantCulture) ?? oldPatient.BirthDate;

        return oldPatient;
    }
}