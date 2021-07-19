using System.Collections.Generic;
using Hl7.Fhir.Model;
using QMUL.DiabetesBackend.DataInterfaces;
using QMUL.DiabetesBackend.ServiceInterfaces;

namespace QMUL.DiabetesBackend.ServiceImpl.Implementations
{
    public class ServiceRequestService : IServiceRequestService
    {
        private readonly IServiceRequestDao serviceRequestDao;

        public ServiceRequestService(IServiceRequestDao serviceRequestDao)
        {
            this.serviceRequestDao = serviceRequestDao;
        }

        public ServiceRequest CreateServiceRequest(ServiceRequest request)
        {
            return this.serviceRequestDao.CreateServiceRequest(request);
        }

        public ServiceRequest GetServiceRequest(string id)
        {
            return this.serviceRequestDao.GetServiceRequest(id);
        }

        public ServiceRequest UpdateServiceRequest(string id, ServiceRequest request)
        {
            var exists = this.serviceRequestDao.GetServiceRequest(id) != null;
            if (exists)
            {
                return this.serviceRequestDao.UpdateServiceRequest(id, request);
            }

            throw new KeyNotFoundException();
        }

        public bool DeleteServiceRequest(string id)
        {
            var exists = this.serviceRequestDao.GetServiceRequest(id) != null;
            if (exists)
            {
                return this.serviceRequestDao.DeleteServiceRequest(id);
            }
            
            throw new KeyNotFoundException();
        }
    }
}