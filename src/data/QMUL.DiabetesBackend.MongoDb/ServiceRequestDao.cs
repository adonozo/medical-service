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

        public Task<ServiceRequest> UpdateServiceRequest(string id, ServiceRequest actualRequest)
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> DeleteServiceRequest(string id)
        {
            throw new System.NotImplementedException();
        }

        public Task<List<ServiceRequest>> GetServiceRequestsByIds(string[] ids)
        {
            throw new System.NotImplementedException();
        }
    }
}