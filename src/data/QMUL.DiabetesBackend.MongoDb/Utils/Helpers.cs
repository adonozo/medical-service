namespace QMUL.DiabetesBackend.MongoDb.Utils;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Model;
using Model.Constants;
using MongoDB.Bson;
using MongoDB.Driver;

public static class Helpers
{
    /// <summary>
    /// Converts a <see cref="Resource"/> object to a <see cref="BsonDocument"/>. The resource is first serialized
    /// to JSON.
    /// </summary>
    /// <param name="resource">The <see cref="Resource"/> to convert.</param>
    /// <returns>A <see cref="BsonDocument"/> parsed from the resource's JSON string.</returns>
    public static async Task<BsonDocument> ToBsonDocumentAsync(Resource resource)
    {
        var json = await resource.ToJsonAsync();
        var bson = BsonDocument.Parse(json);
        return bson ?? throw new ArgumentException("Invalid resource", nameof(resource));
    }

    /// <summary>
    /// Converts an array of <see cref="DataType"/> into a <see cref="BsonArray"/> containing <see cref="BsonDocument"/>
    /// </summary>
    /// <param name="elements">The array of <see cref="DataType"/> from a FHIR object</param>
    /// <returns>A BsonArray of BsonDocuments</returns>
    /// <exception cref="ArgumentException">If any element cannot be serialized into JSON or returns an invalid BSON
    /// object</exception>
    public static BsonArray ToBsonArray(IEnumerable<DataType> elements)
    {
        var json = elements.Select(source =>
        {
            var extensionJson= PocoSerializationExtensions.ToJson(source);
            return BsonDocument.Parse(extensionJson);
        });
        var bson = BsonArray.Create(json);
        return bson ?? throw new ArgumentException("Invalid element", nameof(elements));
    }

    /// <summary>
    /// Gets a <see cref="FilterDefinition{TDocument}"/> with an "eq" operator for the ID.
    /// </summary>
    /// <param name="id">The string ID to look for. This should be a <see cref="ObjectId"/> string equivalent.</param>
    /// <returns>The ID's "eq" filter definition.</returns>
    public static FilterDefinition<BsonDocument> ByIdFilter(string id)
    {
        return ByIdFilter<BsonDocument>(id);
    }

    public static FilterDefinition<T> ByIdFilter<T>(string id)
    {
        var objectId = ObjectId.TryParse(id, out var parsedId) ? parsedId : ObjectId.Empty;
        return Builders<T>.Filter.Eq("_id", objectId);
    }

    /// <summary>
    /// Gets a <see cref="FieldDefinition{TDocument}"/> to filter documents from list of IDs, using the '_id' field
    /// </summary>
    /// <param name="ids">The list of IDs to folter</param>
    /// <returns>A <see cref="FilterDefinition{TDocument}"/> to use in a query</returns>
    public static FilterDefinition<BsonDocument> InIdsFilter(string[] ids)
    {
        var objectIds = ids.Select(id => new ObjectId(id));
        return Builders<BsonDocument>.Filter.In("_id", objectIds);
    }

    /// <summary>
    /// Gets a <see cref="FilterDefinition{TDocument}"/> to equal a subject reference to a patient ID.
    /// </summary>
    /// <param name="patientId">The patient ID.</param>
    /// <returns>The subject reference filter definition.</returns>
    public static FilterDefinition<BsonDocument> PatientReferenceFilter(string patientId)
    {
        var patientReference = Constants.PatientPath + patientId;
        return Builders<BsonDocument>.Filter.Eq("subject.reference", patientReference);
    }

    public static SortDefinition<BsonDocument> DefaultOrder() => Builders<BsonDocument>.Sort.Descending("_id");

    /// <summary>
    /// Sets a <see cref="BsonDocument"/> field as a <see cref="BsonDateTime"/> date from a <see cref="DateTimeOffset"/>.
    /// If the dateTimeOffset is null, the document field will be removed.
    /// </summary>
    /// <param name="document">The <see cref="BsonDocument"/> to modify.</param>
    /// <param name="field">The document's field name.</param>
    /// <param name="dateTimeOffset">The <see cref="DateTimeOffset"/> to use.</param>
    public static void SetBsonDateTimeValue(BsonDocument document, string field, DateTimeOffset? dateTimeOffset)
    {
        if (dateTimeOffset is null)
        {
            document.Remove(field);
            return;
        }

        var dateTime = dateTimeOffset.Value.UtcDateTime;
        document[field] = new BsonDateTime(dateTime);
    }

    /// <summary>
    /// Converts a <see cref="BsonDocument"/> into a <see cref="Resource"/> object. It uses JSON to perform parsing.
    /// </summary>
    /// <param name="document">The <see cref="BsonDocument"/> to convert.</param>
    /// <typeparam name="T">The resource type. Must be a <see cref="Resource"/> child.</typeparam>
    /// <returns>The parsed resource.</returns>
    public static async Task<T> ToResourceAsync<T>(BsonDocument document) where T : Resource
    {
        var id = document["_id"].ToString();
        document.Remove("_id");

        var parser = new FhirJsonParser(new ParserSettings
            { AllowUnrecognizedEnums = true, AcceptUnknownMembers = true, PermissiveParsing = true });
        var resource = await parser.ParseAsync<T>(document.ToJson());
        resource.Id = id;

        return resource;
    }

    /// <summary>
    /// Gets the filter to use with keyset pagination if the last data cursor is not empty.
    /// </summary>
    /// <param name="searchFilters">The filters search filters.</param>
    /// <param name="lastDataCursor">The last ID obtained from previous pagination results.</param>
    /// <returns>The pagination filter.</returns>
    public static FilterDefinition<T> GetPaginationFilter<T>(FilterDefinition<T> searchFilters,
        string lastDataCursor)
    {
        if (string.IsNullOrEmpty(lastDataCursor) || !ObjectId.TryParse(lastDataCursor, out var lastId))
        {
            return searchFilters;
        }

        return Builders<T>.Filter.And(searchFilters,
            Builders<T>.Filter.Lt("_id", lastId));
    }

    /// <summary>
    /// Gets the paginated result including counts, from a resource collection.
    /// </summary>
    /// <param name="collection">The mongo collection.</param>
    /// <param name="searchFilter">The search filters used to get the results.</param>
    /// <param name="results">The <see cref="Resource"/> result array.</param>
    /// <returns>A <see cref="PaginatedResult{T}"/>.</returns>
    public static async Task<PaginatedResult<IEnumerable<TResource>>> GetPaginatedResult<TResource, TDocument>(
        IMongoCollection<TDocument> collection,
        FilterDefinition<TDocument> searchFilter,
        TResource[] results)
    {
        var updatedLastCursorId = GetLastCursorId(results);
        var count = await collection.Find(searchFilter)
            .CountDocumentsAsync();

        var remainingResults = await collection
            .Find(GetPaginationFilter(searchFilter, updatedLastCursorId))
            .CountDocumentsAsync();

        return new PaginatedResult<IEnumerable<TResource>>
        {
            Results = results,
            TotalResults = count,
            LastDataCursor = updatedLastCursorId,
            RemainingCount = remainingResults
        };
    }

    public static async Task<PaginatedResults<TResource>> GetPaginatedResults<TResource, TDocument>(
        IMongoCollection<TDocument> collection,
        FilterDefinition<TDocument> searchFilter,
        TResource[] results)
    {
        var updatedLastCursorId = GetLastCursorId(results);
        var count = await collection.Find(searchFilter)
            .CountDocumentsAsync();

        var remainingResults = await collection
            .Find(GetPaginationFilter(searchFilter, updatedLastCursorId))
            .CountDocumentsAsync();

        return new PaginatedResults<TResource>
        {
            Results = results,
            TotalResults = count,
            LastDataCursor = updatedLastCursorId,
            RemainingCount = remainingResults
        };
    }

    private static string GetLastCursorId<TResource>(TResource[] resources) => resources switch
    {
        Resource[] fhirResources => fhirResources[^1].Id,
        _ => string.Empty
    };
}