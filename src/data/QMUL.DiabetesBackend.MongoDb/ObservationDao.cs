namespace QMUL.DiabetesBackend.MongoDb;

using System.Collections.Generic;
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
public class ObservationDao : MongoDaoBase, IObservationDao
{
    private const string CollectionName = "observation";

    private readonly IMongoCollection<BsonDocument> observationCollection;
    private readonly ILogger<ObservationDao> logger;

    /// <inheritdoc />
    public ObservationDao(IMongoDatabase database, ILogger<ObservationDao> logger) : base(database)
    {
        this.logger = logger;
        this.observationCollection = this.Database.GetCollection<BsonDocument>(CollectionName);
    }

    /// <inheritdoc />
    public async Task<Observation> CreateObservation(Observation observation)
    {
        this.logger.LogDebug("Creating observation...");
        var document = await Helpers.ToBsonDocumentAsync(observation);
        Helpers.SetBsonDateTimeValue(document, "issued", observation.Issued);
        Helpers.SetBsonDateTimeValue(document, "effectiveDateTime", observation.Effective);
        await this.observationCollection.InsertOneAsync(document);

        var newId = this.GetIdFromDocument(document);
        this.logger.LogDebug("Observation created with ID: {Id}", newId);
        const string errorMessage = "Could not create observation";
        document = await this.GetSingleOrThrow(this.observationCollection.Find(Helpers.GetByIdFilter(newId)),
            new WriteResourceException(errorMessage));
        return await ProjectToObservation(document);
    }

    /// <inheritdoc />
    public async Task<Observation?> GetObservation(string id)
    {
        var document = await this.observationCollection.Find(Helpers.GetByIdFilter(id)).FirstOrDefaultAsync();
        if (document is null)
        {
            return null;
        }

        return await ProjectToObservation(document);
    }

    /// <inheritdoc />
    public async Task<PaginatedResult<IEnumerable<Resource>>> GetObservationsFor(string patientId,
        PaginationRequest paginationRequest,
        Instant? start = null,
        Instant? end = null)
    {
        var searchFilter = Builders<BsonDocument>.Filter.And(Helpers.GetPatientReferenceFilter(patientId));

        if (start.HasValue)
        {
            searchFilter &= Builders<BsonDocument>.Filter.Gt("issued", start);
        }

        if (end.HasValue)
        {
            searchFilter &= Builders<BsonDocument>.Filter.Lt("issued", end);
        }

        var resultsFilter = Helpers.GetPaginationFilter(searchFilter, paginationRequest.LastCursorId);
        var result = await this.observationCollection.Find(resultsFilter)
            .Limit(paginationRequest.Limit)
            .Sort(Builders<BsonDocument>.Sort.Descending("effectiveDateTime"))
            .Sort(Helpers.GetDefaultOrder())
            .Project(document => ProjectToObservation(document))
            .ToListAsync();

        Resource[] observations = await Task.WhenAll(result);
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
            .ReplaceOneAsync(Helpers.GetByIdFilter(id), document);

        return result.IsAcknowledged;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteObservation(string id)
    {
        this.logger.LogDebug("Deleting Observation with ID: {Id}", id);
        var result = await this.observationCollection.DeleteOneAsync(Helpers.GetByIdFilter(id));
        return result.IsAcknowledged;
    }

    private static async Task<Observation> ProjectToObservation(BsonDocument document)
    {
        document["issued"] = document["issued"].ToString();
        document["effectiveDateTime"] = document["effectiveDateTime"].ToString();
        return await Helpers.ToResourceAsync<Observation>(document);
    }
}