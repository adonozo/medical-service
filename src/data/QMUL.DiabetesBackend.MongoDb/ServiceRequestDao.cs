namespace QMUL.DiabetesBackend.MongoDb;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using DataInterfaces;
using Hl7.Fhir.Model;
using Hl7.Fhir.Utility;
using Microsoft.Extensions.Logging;
using Model.Constants;
using Model.Exceptions;
using MongoDB.Bson;
using MongoDB.Driver;
using NodaTime;
using Utils;
using Task = System.Threading.Tasks.Task;

/// <summary>
/// The Service Request Dao
/// </summary>
public class ServiceRequestDao : MongoMultiLingualBase, IServiceRequestDao
{
    private const string CollectionName = "serviceRequest";

    private readonly IMongoCollection<BsonDocument> serviceRequestCollection;
    private readonly ILogger<ServiceRequestDao> logger;

    public ServiceRequestDao(IMongoDatabase database, ILogger<ServiceRequestDao> logger) : base(database)
    {
        this.logger = logger;
        this.serviceRequestCollection = this.GetLocalizedCollection(CollectionName);
    }

    /// <inheritdoc />
    public async Task<ServiceRequest> CreateServiceRequest(ServiceRequest newRequest)
    {
        this.logger.LogDebug("Creating new service request");
        var document = await Helpers.ToBsonDocumentAsync(newRequest);
        await this.serviceRequestCollection.InsertOneAsync(document);

        var newId = this.GetIdFromDocument(document);
        this.logger.LogDebug("Service request created with ID: {Id}", newId);
        const string exceptionMessage = "Could not create the service request";
        return await this.GetSingleRequestOrThrow(newId, new WriteResourceException(exceptionMessage));
    }

    /// <inheritdoc />
    public async Task<ServiceRequest?> GetServiceRequest(string id)
    {
        var document = await this.serviceRequestCollection.Find(Helpers.ByIdFilter(id)).FirstOrDefaultAsync();
        if (document is null)
        {
            return null;
        }

        return await Helpers.ToResourceAsync<ServiceRequest>(document);
    }

    /// <inheritdoc />
    public async Task<IList<ServiceRequest>> GetActiveServiceRequests(string patientId)
    {
        this.logger.LogDebug("Getting service active service requests for {PatientId}", patientId);
        var filters = Builders<BsonDocument>.Filter.And(
            Helpers.PatientReferenceFilter(patientId),
            Builders<BsonDocument>.Filter.Eq("status", RequestStatus.Active.GetLiteral()));
        var documents = await this.serviceRequestCollection.Find(filters)
            .ToListAsync();
        var results = documents.Select(Helpers.ToResourceAsync<ServiceRequest>);

        var serviceRequests = await Task.WhenAll(results);
        this.logger.LogDebug("Found {Count} service request(s) for {Id}", serviceRequests.Length, patientId);
        return serviceRequests;
    }

    /// <inheritdoc />
    public async Task<IList<ServiceRequest>> GetServiceRequestsByIds(string[] ids)
    {
        var documents = await this.serviceRequestCollection.Find(Helpers.InIdsFilter(ids))
            .ToListAsync();
        var results = documents.Select(Helpers.ToResourceAsync<ServiceRequest>);
        return await Task.WhenAll(results);
    }

    /// <inheritdoc />
    public async Task<bool> UpdateServiceRequest(string id, ServiceRequest actualRequest)
    {
        logger.LogDebug("Updating service request {Id}", id);
        var document = await Helpers.ToBsonDocumentAsync(actualRequest);
        var result = await this.serviceRequestCollection.ReplaceOneAsync(Helpers.ByIdFilter(id), document);

        return result.IsAcknowledged;
    }

    /// <inheritdoc />
    public async Task<bool> UpdateServiceRequestsStatus(string[] ids, RequestStatus status)
    {
        var filter = Helpers.InIdsFilter(ids);
        var update = Builders<BsonDocument>.Update.Set("status", status.ToString().ToLowerInvariant());
        var result = await this.serviceRequestCollection.UpdateManyAsync(filter, update);
        return result.IsAcknowledged;
    }

    /// <inheritdoc />
    public async Task<bool> UpdateServiceRequestsStartDate(string[] ids, LocalDate date)
    {
        var filter = Helpers.InIdsFilter(ids);
        var extension = CreateStartDateExtension(date);
        var document = Helpers.ToBsonArray(new[] { extension });
        var update = Builders<BsonDocument>.Update.Set("occurrenceTiming.extension", document);
        var result = await this.serviceRequestCollection.UpdateManyAsync(filter, update);
        return result.IsAcknowledged;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteServiceRequest(string id)
    {
        this.logger.LogDebug("Deleting service requests with ID: {Id}", id);
        var result = await this.serviceRequestCollection.DeleteOneAsync(Helpers.ByIdFilter(id));
        return result.IsAcknowledged;
    }
    
    /// <inheritdoc />
    public async Task<bool> DeleteServiceRequests(string[] ids)
    {
        var result = await this.serviceRequestCollection.DeleteManyAsync(Helpers.InIdsFilter(ids));
        return result.IsAcknowledged;
    }

    private async Task<ServiceRequest> GetSingleRequestOrThrow(string id, Exception exception)
    {
        var cursor = this.serviceRequestCollection.Find(Helpers.ByIdFilter(id));
        var document = await this.GetSingleOrThrow(cursor, exception);
        return await Helpers.ToResourceAsync<ServiceRequest>(document);
    }

    private static Extension CreateStartDateExtension(LocalDate date)
    {
        var fhirString = new FhirString(date.ToString("R", CultureInfo.InvariantCulture));
        return new Extension(Extensions.TimingStartDate, fhirString);
    }
}