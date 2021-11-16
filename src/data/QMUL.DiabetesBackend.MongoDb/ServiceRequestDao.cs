namespace QMUL.DiabetesBackend.MongoDb
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using DataInterfaces;
    using Hl7.Fhir.Model;
    using Microsoft.Extensions.Logging;
    using Model;
    using Models;
    using MongoDB.Driver;
    using Utils;

    /// <summary>
    /// The Service Request Dao
    /// </summary>
    public class ServiceRequestDao : MongoDaoBase, IServiceRequestDao
    {
        private readonly IMongoCollection<MongoServiceRequest> serviceRequestCollection;
        private const string CollectionName = "serviceRequest";
        private readonly ILogger<ServiceRequestDao> logger;
        
        public ServiceRequestDao(IDatabaseSettings settings, ILogger<ServiceRequestDao> logger) : base(settings)
        {
            this.logger = logger;
            this.serviceRequestCollection = this.Database.GetCollection<MongoServiceRequest>(CollectionName);
        }

        /// <inheritdoc />
        public async Task<ServiceRequest> CreateServiceRequest(ServiceRequest newRequest)
        {
            this.logger.LogDebug("Creating new service request");
            var mongoRequest = newRequest.ToMongoServiceRequest();
            mongoRequest.CreateAt = DateTime.UtcNow;
            await this.serviceRequestCollection.InsertOneAsync(mongoRequest);
            this.logger.LogDebug("Service request created with ID: {Id}", mongoRequest.Id);
            return await this.GetServiceRequest(mongoRequest.Id);
        }

        /// <inheritdoc />
        public async Task<ServiceRequest> GetServiceRequest(string id)
        {
            var result = this.serviceRequestCollection.Find(request => request.Id == id)
                .Project(request => request.ToServiceRequest());
            return await result.FirstOrDefaultAsync();
        }

        /// <inheritdoc />
        public async Task<List<ServiceRequest>> GetServiceRequestsFor(string patientId)
        {
            this.logger.LogTrace("Getting service requests for {PatientId}", patientId);
            var result = this.serviceRequestCollection.Find(request =>
                    request.PatientReference.ReferenceId == patientId)
                .Project(mongoServiceRequest => mongoServiceRequest.ToServiceRequest());
            var serviceRequests = await result.ToListAsync();
            this.logger.LogTrace("Found {Count} service request(s) for {Id}", serviceRequests.Count, patientId);
            return serviceRequests;
        }

        /// <inheritdoc />
        public async Task<List<ServiceRequest>> GetActiveServiceRequests(string patientId)
        {
            this.logger.LogTrace("Getting service active service requests for {PatientId}", patientId);
            var result = this.serviceRequestCollection.Find(request =>
                    request.PatientReference.ReferenceId == patientId
                    && request.Status == RequestStatus.Active.ToString())
                .Project(mongoServiceRequest => mongoServiceRequest.ToServiceRequest());
            var serviceRequests = await result.ToListAsync();
            this.logger.LogTrace("Found {Count} service request(s) for {Id}", serviceRequests.Count, patientId);
            return serviceRequests;
        }

        /// <inheritdoc />
        public async Task<List<ServiceRequest>> GetServiceRequestsByIds(string[] ids)
        {
            var idFilter = Builders<MongoServiceRequest>.Filter
                .In(item => item.Id, ids);
            var cursor = this.serviceRequestCollection.Find(idFilter)
                .Project(mongoServiceRequest => mongoServiceRequest.ToServiceRequest());
            return await cursor.ToListAsync();
        }

        /// <inheritdoc />
        public async Task<ServiceRequest> UpdateServiceRequest(string id, ServiceRequest actualRequest)
        {
            logger.LogDebug("Updating service request {Id}", id);
            var mongoRequests = actualRequest.ToMongoServiceRequest();
            var result = await this.serviceRequestCollection.ReplaceOneAsync(request => request.Id == id, mongoRequests);
            if (!result.IsAcknowledged)
            {
                throw new InvalidOperationException($"there was an error updating the Medication Request {id}");
            }

            this.logger.LogDebug("Service Request with ID {Id} updated", id);
            return await this.GetServiceRequest(id);
        }

        /// <inheritdoc />
        public async Task<bool> DeleteServiceRequest(string id)
        {
            this.logger.LogDebug("Deleting service requests with ID: {Id}", id);
            var result = await this.serviceRequestCollection.DeleteOneAsync(request => request.Id == id);
            return result.IsAcknowledged;
        }
    }
}