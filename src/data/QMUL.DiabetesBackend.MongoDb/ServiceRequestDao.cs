using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using MongoDB.Driver;
using QMUL.DiabetesBackend.DataInterfaces;
using QMUL.DiabetesBackend.Model;
using QMUL.DiabetesBackend.MongoDb.Models;
using QMUL.DiabetesBackend.MongoDb.Utils;

namespace QMUL.DiabetesBackend.MongoDb
{
    public class ServiceRequestDao : MongoDaoBase, IServiceRequestDao
    {
        private readonly IMongoCollection<MongoServiceRequest> serviceRequestCollection;
        private const string CollectionName = "serviceRequest";
        
        public ServiceRequestDao(IDatabaseSettings settings) : base(settings)
        {
            this.serviceRequestCollection = this.Database.GetCollection<MongoServiceRequest>(CollectionName);
        }

        public async Task<ServiceRequest> CreateServiceRequest(ServiceRequest newRequest)
        {
            var mongoRequest = newRequest.ToMongoServiceRequest();
            mongoRequest.CreateAt = DateTime.UtcNow;
            await this.serviceRequestCollection.InsertOneAsync(mongoRequest);
            return await this.GetServiceRequest(mongoRequest.Id);
        }

        public async Task<ServiceRequest> GetServiceRequest(string id)
        {
            var result = this.serviceRequestCollection.Find(request => request.Id == id)
                .Project(request => request.ToServiceRequest());
            return await result.FirstOrDefaultAsync();
        }

        public async Task<List<ServiceRequest>> GetServiceRequestsFor(string patientId)
        {
            var result = this.serviceRequestCollection.Find(request =>
                    request.PatientReference.ReferenceId == patientId)
                .Project(mongoServiceRequest => mongoServiceRequest.ToServiceRequest());
            return await result.ToListAsync();
        }

        public async Task<List<ServiceRequest>> GetActiveServiceRequests(string patientId)
        {
            var result = this.serviceRequestCollection.Find(request =>
                    request.PatientReference.ReferenceId == patientId
                    && request.Status == RequestStatus.Active.ToString())
                .Project(mongoServiceRequest => mongoServiceRequest.ToServiceRequest());
            return await result.ToListAsync();
        }

        public async Task<List<ServiceRequest>> GetServiceRequestsByIds(string[] ids)
        {
            var idFilter = Builders<MongoServiceRequest>.Filter
                .In(item => item.Id, ids);
            var cursor = this.serviceRequestCollection.Find(idFilter)
                .Project(mongoServiceRequest => mongoServiceRequest.ToServiceRequest());
            return await cursor.ToListAsync();
        }

        public async Task<ServiceRequest> UpdateServiceRequest(string id, ServiceRequest actualRequest)
        {
            var mongoRequests = actualRequest.ToMongoServiceRequest();
            var result = await this.serviceRequestCollection.ReplaceOneAsync(request => request.Id == id, mongoRequests);
            if (result.IsAcknowledged)
            {
                return await this.GetServiceRequest(id);
            }

            throw new InvalidOperationException($"there was an error updating the Medication Request {id}");
        }

        public async Task<bool> DeleteServiceRequest(string id)
        {
            var result = await this.serviceRequestCollection.DeleteOneAsync(request => request.Id == id);
            return result.IsAcknowledged;
        }
    }
}