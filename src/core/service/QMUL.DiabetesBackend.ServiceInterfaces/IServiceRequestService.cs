namespace QMUL.DiabetesBackend.ServiceInterfaces
{
    using System.Threading.Tasks;
    using Hl7.Fhir.Model;

    /// <summary>
    /// The Service Request Service Interface
    /// </summary>
    public interface IServiceRequestService
    {
        public Task<ServiceRequest> CreateServiceRequest(ServiceRequest request);

        public Task<ServiceRequest> GetServiceRequest(string id);

        public Task<ServiceRequest> UpdateServiceRequest(string id, ServiceRequest request);

        public Task<bool> DeleteServiceRequest(string id);
    }
}