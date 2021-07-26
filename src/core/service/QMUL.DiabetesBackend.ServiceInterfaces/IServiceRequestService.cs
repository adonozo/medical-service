using Hl7.Fhir.Model;

namespace QMUL.DiabetesBackend.ServiceInterfaces
{
    public interface IServiceRequestService
    {
        public ServiceRequest CreateServiceRequest(ServiceRequest request);

        public ServiceRequest GetServiceRequest(string id);

        public ServiceRequest UpdateServiceRequest(string id, ServiceRequest request);

        public bool DeleteServiceRequest(string id);
    }
}