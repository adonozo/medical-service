using Hl7.Fhir.Model;

namespace QMUL.DiabetesBackend.DataInterfaces
{
    public interface IServiceRequestDao
    {
        public ServiceRequest CreateServiceRequest(ServiceRequest newRequest);
        
        public ServiceRequest GetServiceRequest(string id);

        public ServiceRequest UpdateServiceRequest(string id, ServiceRequest actualRequest);

        public bool DeleteServiceRequest(string id);
    }
}