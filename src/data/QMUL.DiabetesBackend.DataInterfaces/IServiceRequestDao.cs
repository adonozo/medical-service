namespace QMUL.DiabetesBackend.DataInterfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Hl7.Fhir.Model;

    /// <summary>
    /// The Service Request Dao interface.
    /// </summary>
    public interface IServiceRequestDao
    {
        public Task<ServiceRequest> CreateServiceRequest(ServiceRequest newRequest);
        
        public Task<ServiceRequest> GetServiceRequest(string id);

        public Task<List<ServiceRequest>> GetServiceRequestsFor(string patientId);

        public Task<List<ServiceRequest>> GetActiveServiceRequests(string patientId);

        public Task<ServiceRequest> UpdateServiceRequest(string id, ServiceRequest actualRequest);

        public Task<bool> DeleteServiceRequest(string id);
        
        public Task<List<ServiceRequest>> GetServiceRequestsByIds(string[] ids);
    }
}