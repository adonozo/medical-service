namespace QMUL.DiabetesBackend.MongoDb;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataInterfaces;
using Hl7.Fhir.Model;
using Microsoft.Extensions.Logging;
using Model;
using Model.Exceptions;
using MongoDB.Bson;
using MongoDB.Driver;
using Utils;
using Instant = NodaTime.Instant;
using Task = System.Threading.Tasks.Task;

/// <summary>
/// The Observation Dao
/// </summary>
public class ObservationDao : MongoMultiLingualBase, IObservationDao
{
    private const string CollectionName = "observation";

    private readonly IMongoCollection<BsonDocument> observationCollection;
    private readonly ILogger<ObservationDao> logger;

    /// <inheritdoc />
    public ObservationDao(IMongoDatabase database, ILogger<ObservationDao> logger) : base(database)
    {
        this.logger = logger;
        this.observationCollection = this.GetLocalizedCollection(CollectionName);
    }

    /// <inheritdoc />
    public async Task<Observation> CreateObservation(Observation observation)
    {
        this.logger.LogDebug("Creating observation...");
        var document = await Helpers.ToBsonDocumentAsync(observation);
        Helpers.SetBsonDateTimeValue(document, "issued", observation.Issued);
        await this.observationCollection.InsertOneAsync(document);

        var newId = this.GetIdFromDocument(document);
        this.logger.LogDebug("Observation created with ID: {Id}", newId);
        const string errorMessage = "Could not create observation";
        document = await this.GetSingleOrThrow(this.observationCollection.Find(Helpers.ByIdFilter(newId)),
            new WriteResourceException(errorMessage));
        return await this.ProjectToObservation(document);
    }

    /// <inheritdoc />
    public async Task<Observation?> GetObservation(string id)
    {
        var document = await this.observationCollection.Find(Helpers.ByIdFilter(id)).FirstOrDefaultAsync();
        if (document is null)
        {
            return null;
        }

        return await this.ProjectToObservation(document);
    }

    /// <inheritdoc />
    public async Task<PaginatedResult<IEnumerable<Resource>>> GetAllObservationsFor(string patientId,
        PaginationRequest paginationRequest)
    {
        var searchFilter = Helpers.PatientReferenceFilter(patientId);
        var resultsFilter = Helpers.GetPaginationFilter(searchFilter, paginationRequest.LastCursorId);

        var documents = await this.observationCollection.Find(resultsFilter)
            .Limit(paginationRequest.Limit)
            .Sort(Helpers.DefaultOrder())
            .ToListAsync();
        var results = documents.Select(this.ProjectToObservation);
        Resource[] observations = await Task.WhenAll(results);

        if (observations.Length == 0)
        {
            return new PaginatedResult<IEnumerable<Resource>> { Results = observations };
        }

        return await Helpers.GetPaginatedResult(this.observationCollection, searchFilter, observations);
    }

    /// <inheritdoc />
    public async Task<PaginatedResult<IEnumerable<Resource>>> GetObservationsFor(string patientId,
        PaginationRequest paginationRequest,
        Instant? start = null,
        Instant? end = null)
    {
        var searchFilter = Builders<BsonDocument>.Filter.And(Helpers.PatientReferenceFilter(patientId));

        if (start.HasValue)
        {
            searchFilter &= Builders<BsonDocument>.Filter.Gt("issued", start);
        }

        if (end.HasValue)
        {
            searchFilter &= Builders<BsonDocument>.Filter.Lt("issued", end);
        }

        var resultsFilter = Helpers.GetPaginationFilter(searchFilter, paginationRequest.LastCursorId);
        var documents = await this.observationCollection.Find(resultsFilter)
            .Limit(paginationRequest.Limit)
            .ToListAsync();

        var results = documents.Select(this.ProjectToObservation);
        Resource[] observations = await Task.WhenAll(results);
        if (observations.Length == 0)
        {
            return new PaginatedResult<IEnumerable<Resource>> { Results = observations };
        }

        return await Helpers.GetPaginatedResult(this.observationCollection, searchFilter, observations);
    }

    /// <inheritdoc />
    public async Task<bool> UpdateObservation(string id, Observation observation)
    {
        this.logger.LogDebug("Updating Observation with ID {Id}", id);
        var document = await Helpers.ToBsonDocumentAsync(observation);
        var result = await this.observationCollection
            .ReplaceOneAsync(Helpers.ByIdFilter(id), document);

        return result.IsAcknowledged;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteObservation(string id)
    {
        this.logger.LogDebug("Deleting Observation with ID: {Id}", id);
        var result = await this.observationCollection.DeleteOneAsync(Helpers.ByIdFilter(id));
        return result.IsAcknowledged;
    }

    private async Task<Observation> ProjectToObservation(BsonDocument document)
    {
        document["issued"] = document["issued"].ToString();
        document["effectiveDateTime"] = document["effectiveDateTime"].ToString();
        return await Helpers.ToResourceAsync<Observation>(document);
    }
}