namespace QMUL.DiabetesBackend.MongoDb
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using DataInterfaces;
    using DataInterfaces.Exceptions;
    using Hl7.Fhir.Model;
    using Hl7.Fhir.Utility;
    using Microsoft.Extensions.Logging;
    using MongoDB.Bson;
    using MongoDB.Driver;
    using Utils;
    using Task = System.Threading.Tasks.Task;

    /// <summary>
    /// The Service Request Dao
    /// </summary>
    public class ServiceRequestDao : MongoDaoBase, IServiceRequestDao
    {
        private const string CollectionName = "serviceRequest";

        private readonly IMongoCollection<BsonDocument> serviceRequestCollection;
        private readonly ILogger<ServiceRequestDao> logger;

        public ServiceRequestDao(IMongoDatabase database, ILogger<ServiceRequestDao> logger) : base(database)
        {
            this.logger = logger;
            this.serviceRequestCollection = this.Database.GetCollection<BsonDocument>(CollectionName);
        }

        /// <inheritdoc />
        public async Task<ServiceRequest> CreateServiceRequest(ServiceRequest newRequest)
        {
            this.logger.LogDebug("Creating new service request");
            var document = await Helpers.ToBsonDocumentAsync(newRequest);
            await this.serviceRequestCollection.InsertOneAsync(document);

            var newId = document["_id"].ToString();
            this.logger.LogDebug("Service request created with ID: {Id}", newId);
            const string exceptionMessage = "Could not create the service request";
            return await this.GetSingleRequestOrThrow(newId, new CreateException(exceptionMessage));
        }

        /// <inheritdoc />
        public async Task<ServiceRequest> GetServiceRequest(string id)
        {
            var notFoundMessage = $"Service Request {id} not found";
            return await this.GetSingleRequestOrThrow(id, new NotFoundException(notFoundMessage));
        }

        /// <inheritdoc />
        public async Task<IList<ServiceRequest>> GetServiceRequestsFor(string patientId)
        {
            this.logger.LogTrace("Getting service requests for {PatientId}", patientId);
            var results = await this.serviceRequestCollection.Find(Helpers.GetPatientReferenceFilter(patientId))
                .Project(document => Helpers.ToResourceAsync<ServiceRequest>(document))
                .ToListAsync();
            
            var serviceRequests = await Task.WhenAll(results);
            this.logger.LogTrace("Found {Count} service request(s) for {Id}", serviceRequests.Length, patientId);
            return serviceRequests;
        }

        /// <inheritdoc />
        public async Task<IList<ServiceRequest>> GetActiveServiceRequests(string patientId)
        {
            this.logger.LogTrace("Getting service active service requests for {PatientId}", patientId);
            var filters = Builders<BsonDocument>.Filter.And(
                Helpers.GetPatientReferenceFilter(patientId),
                Builders<BsonDocument>.Filter.Eq("status", RequestStatus.Active.GetLiteral()));
            var results = await this.serviceRequestCollection.Find(filters)
                .Project(document => Helpers.ToResourceAsync<ServiceRequest>(document))
                .ToListAsync();

            var serviceRequests = await Task.WhenAll(results);
            this.logger.LogTrace("Found {Count} service request(s) for {Id}", serviceRequests.Length, patientId);
            return serviceRequests;
        }

        /// <inheritdoc />
        public async Task<IList<ServiceRequest>> GetServiceRequestsByIds(string[] ids)
        {
            var idFilter = Builders<BsonDocument>.Filter
                .In("_id", ids);
            var results = await this.serviceRequestCollection.Find(idFilter)
                .Project(document => Helpers.ToResourceAsync<ServiceRequest>(document))
                .ToListAsync();
            return await Task.WhenAll(results);
        }

        /// <inheritdoc />
        public async Task<ServiceRequest> UpdateServiceRequest(string id, ServiceRequest actualRequest)
        {
            logger.LogDebug("Updating service request {Id}", id);
            var document = await Helpers.ToBsonDocumentAsync(actualRequest);
            var result = await this.serviceRequestCollection.ReplaceOneAsync(Helpers.GetByIdFilter(id), document);

            var errorMessage = $"Could not update service request with ID {id}";
            this.CheckAcknowledgedOrThrow(result.IsAcknowledged, new UpdateException(errorMessage),
                () => this.logger.LogWarning("{ErrorMessage}", errorMessage));
            this.logger.LogDebug("Service Request with ID {Id} updated", id);
            return await this.GetSingleRequestOrThrow(id, new UpdateException(errorMessage));
        }

        /// <inheritdoc />
        public async Task<bool> DeleteServiceRequest(string id)
        {
            this.logger.LogDebug("Deleting service requests with ID: {Id}", id);
            var result = await this.serviceRequestCollection.DeleteOneAsync(Helpers.GetByIdFilter(id));
            return result.IsAcknowledged;
        }

        private async Task<ServiceRequest> GetSingleRequestOrThrow(string id, DataExceptionBase exception,
            Action fallback = null)
        {
            var cursor = this.serviceRequestCollection.Find(Helpers.GetByIdFilter(id));
            var document = await this.GetSingleOrThrow(cursor, exception, fallback);
            return await Helpers.ToResourceAsync<ServiceRequest>(document);
        }
    }
}