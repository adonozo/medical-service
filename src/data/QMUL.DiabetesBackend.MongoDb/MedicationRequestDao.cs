namespace QMUL.DiabetesBackend.MongoDb;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataInterfaces;
using Hl7.Fhir.Model;
using Hl7.Fhir.Utility;
using Microsoft.Extensions.Logging;
using Model;
using Model.Exceptions;
using Model.Extensions;
using MongoDB.Bson;
using MongoDB.Driver;
using Utils;
using Task = System.Threading.Tasks.Task;

/// <summary>
/// The Medication Request Dao
/// </summary>
public class MedicationRequestDao : MongoDaoBase, IMedicationRequestDao
{
    private const string CollectionName = "medicationRequest";

    private readonly IMongoCollection<BsonDocument> medicationRequestCollection;
    private readonly ILogger<MedicationRequestDao> logger;

    public MedicationRequestDao(IMongoDatabase database, ILogger<MedicationRequestDao> logger) : base(database)
    {
        this.logger = logger;
        this.medicationRequestCollection = this.Database.GetCollection<BsonDocument>(CollectionName);
    }

    /// <inheritdoc />
    public async Task<MedicationRequest> CreateMedicationRequest(MedicationRequest newRequest)
    {
        this.logger.LogDebug("Creating medication request");
        SetDosageId(newRequest, true);

        var document = await RequestToBsonDocument(newRequest);
        await this.medicationRequestCollection.InsertOneAsync(document);
        var newId = this.GetIdFromDocument(document);

        this.logger.LogDebug("Medication request created with ID {Id}", newId);
        var errorMessage = $"The medication request was not created";
        return await this.GetSingleMedicationRequestOrThrow(newId, new WriteResourceException(errorMessage));
    }

    /// <inheritdoc />
    public async Task<bool> UpdateMedicationRequest(string id, MedicationRequest actualRequest)
    {
        this.logger.LogDebug("Updating medication request with ID {Id}", id);
        SetDosageId(actualRequest);

        var document = await RequestToBsonDocument(actualRequest);
        var result = await this.medicationRequestCollection
            .ReplaceOneAsync(Helpers.GetByIdFilter(id), document);

        return result.IsAcknowledged;
    }

    /// <inheritdoc />
    public async Task<MedicationRequest?> GetMedicationRequest(string id)
    {
        var document = await this.medicationRequestCollection.Find(Helpers.GetByIdFilter(id)).FirstOrDefaultAsync();
        if (document is null)
        {
            return null;
        }

        return await Helpers.ToResourceAsync<MedicationRequest>(document);
    }

    /// <inheritdoc />
    public async Task<IList<MedicationRequest>> GetMedicationRequestsByIds(string[] ids)
    {
        var objectIds = ids.Select(id => new ObjectId(id));
        var idFilter = Builders<BsonDocument>.Filter.In("_id", objectIds);
        var results = await this.medicationRequestCollection.Find(idFilter)
            .Project(document => Helpers.ToResourceAsync<MedicationRequest>(document))
            .ToListAsync();
        return await Task.WhenAll(results);
    }

    /// <inheritdoc />
    public async Task<IList<MedicationRequest>> GetMedicationRequestFor(string patientId)
    {
        var filter = Helpers.GetPatientReferenceFilter(patientId);
        var results = await this.medicationRequestCollection.Find(filter)
            .Project(document => Helpers.ToResourceAsync<MedicationRequest>(document))
            .ToListAsync();
        return await Task.WhenAll(results);
    }

    /// <inheritdoc />
    public async Task<bool> DeleteMedicationRequest(string id)
    {
        this.logger.LogDebug("Deleting medication request with ID: {Id}", id);
        var result = await this.medicationRequestCollection.DeleteOneAsync(Helpers.GetByIdFilter(id));
        return result.IsAcknowledged;
    }

    /// <inheritdoc />
    public async Task<MedicationRequest?> GetMedicationRequestForDosage(string patientId, string dosageId)
    {
        var filters = Builders<BsonDocument>.Filter.And(
            Helpers.GetPatientReferenceFilter(patientId),
            Builders<BsonDocument>.Filter.Eq("dosageInstruction.id", dosageId));
        var document = await this.medicationRequestCollection.Find(filters).FirstOrDefaultAsync();
        if (document is null)
        {
            return null;
        }

        return await Helpers.ToResourceAsync<MedicationRequest>(document);
    }

    /// <inheritdoc />
    public async Task<PaginatedResult<IEnumerable<Resource>>> GetActiveMedicationRequests(string patientId,
        PaginationRequest paginationRequest)
    {
        var searchFilter = Builders<BsonDocument>.Filter.And(
            Helpers.GetPatientReferenceFilter(patientId),
            Builders<BsonDocument>.Filter.Eq("isInsulin", false),
            Builders<BsonDocument>.Filter.Eq("status",
                MedicationRequest.medicationrequestStatus.Active.GetLiteral()));
        var resultFilters = Helpers.GetPaginationFilter(searchFilter, paginationRequest.LastCursorId);

        var results = await this.medicationRequestCollection.Find(resultFilters)
            .Project(document => Helpers.ToResourceAsync<MedicationRequest>(document))
            .ToListAsync();
        Resource[] medicationRequests = await Task.WhenAll(results);
        this.logger.LogTrace("Found {Count} medications", medicationRequests.Length);
        if (medicationRequests.Length == 0)
        {
            return new PaginatedResult<IEnumerable<Resource>> { Results = medicationRequests };
        }

        return await Helpers.GetPaginatedResult(this.medicationRequestCollection, searchFilter, medicationRequests);
    }

    /// <inheritdoc />
    public async Task<IList<MedicationRequest>> GetAllActiveMedicationRequests(string patientId)
    {
        var filters = Builders<BsonDocument>.Filter.And(
            Helpers.GetPatientReferenceFilter(patientId),
            Builders<BsonDocument>.Filter.Eq("status",
                MedicationRequest.medicationrequestStatus.Active.GetLiteral()));

        var results = await this.medicationRequestCollection.Find(filters)
            .Project(document => Helpers.ToResourceAsync<MedicationRequest>(document))
            .ToListAsync();
        return await Task.WhenAll(results);
    }

    private async Task<MedicationRequest> GetSingleMedicationRequestOrThrow(string id, Exception exception)
    {
        var cursor = this.medicationRequestCollection.Find(Helpers.GetByIdFilter(id));
        var document = await this.GetSingleOrThrow(cursor, exception);
        return await Helpers.ToResourceAsync<MedicationRequest>(document);
    }

    private static void SetDosageId(MedicationRequest request, bool force = false)
    {
        var emptyDosageIds = request.DosageInstruction
            .Where(dosage => force || string.IsNullOrWhiteSpace(dosage.ElementId));
        foreach (var dosage in emptyDosageIds)
        {
            dosage.ElementId = ObjectId.GenerateNewId().ToString();
        }
    }

    private static async Task<BsonDocument> RequestToBsonDocument(MedicationRequest request)
    {
        var document = await Helpers.ToBsonDocumentAsync(request);
        document["isInsulin"] = request.HasInsulinFlag();
        return document;
    }
}