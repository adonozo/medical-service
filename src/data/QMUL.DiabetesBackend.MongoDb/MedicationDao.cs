namespace QMUL.DiabetesBackend.MongoDb;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using MongoDB.Bson;
using MongoDB.Driver;
using DataInterfaces;
using Microsoft.Extensions.Logging;
using Model;
using Model.Exceptions;
using Utils;
using Task = System.Threading.Tasks.Task;

/// <summary>
/// The Mongo Medication Dao
/// </summary>
public class MedicationDao : MongoMultiLingualBase, IMedicationDao
{
    private const string CollectionName = "medication";

    private readonly IMongoCollection<BsonDocument> medicationCollection;
    private readonly ILogger<MedicationDao> logger;

    public MedicationDao(IMongoDatabase database, ILogger<MedicationDao> logger) : base(database)
    {
        this.logger = logger;
        this.medicationCollection = this.GetLocalizedCollection(CollectionName);
    }

    /// <inheritdoc />
    public async Task<PaginatedResult<IEnumerable<Resource>>> GetMedicationList(PaginationRequest paginationRequest,
        string? name = null)
    {
        this.logger.LogTrace("Getting all medications...");
        var searchFilter = name is null
            ? FilterDefinition<BsonDocument>.Empty
            : Builders<BsonDocument>.Filter.Regex("code.coding.display", new BsonRegularExpression(name, "i"));
        var resultsFilter = Helpers.GetPaginationFilter(searchFilter, paginationRequest.LastCursorId);
        var documents = await this.medicationCollection.Find(resultsFilter)
            .Limit(paginationRequest.Limit)
            .Sort(Helpers.DefaultOrder())
            .ToListAsync();

        var results = documents.Select(Helpers.ToResourceAsync<Medication>);
        Resource[] medications = await Task.WhenAll(results);
        this.logger.LogTrace("Found {Count} medications", medications.Length);
        if (medications.Length == 0)
        {
            return new PaginatedResult<IEnumerable<Resource>> { Results = Array.Empty<Resource>() };
        }

        return await Helpers.GetPaginatedResult(this.medicationCollection, searchFilter, medications);
    }

    /// <inheritdoc />
    public async Task<Medication?> GetSingleMedication(string id)
    {
        var result = await this.medicationCollection.Find(Helpers.ByIdFilter(id)).FirstOrDefaultAsync();
        if (result is null)
        {
            return null;
        }

        return await Helpers.ToResourceAsync<Medication>(result);
    }

    /// <inheritdoc />
    public async Task<Medication> CreateMedication(Medication newMedication)
    {
        this.logger.LogDebug("Creating medication...");
        var document = await Helpers.ToBsonDocumentAsync(newMedication);
        await this.medicationCollection.InsertOneAsync(document);

        var newId = this.GetIdFromDocument(document);
        this.logger.LogDebug("Medication created with ID: {Id}", newId);
        var errorMessage = $"Could not create medication. ID assigned was: {newId}";
        return await this.GetSingleMedicationOrThrow(newId, new WriteResourceException(errorMessage));
    }

    private async Task<Medication> GetSingleMedicationOrThrow(string id, Exception exception)
    {
        var result = this.medicationCollection.Find(Helpers.ByIdFilter(id));
        var document = await this.GetSingleOrThrow(result, exception);
        return await Helpers.ToResourceAsync<Medication>(document);
    }
}