using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using QMUL.DiabetesBackend.DataInterfaces;
using QMUL.DiabetesBackend.ServiceImpl.Utils;
using QMUL.DiabetesBackend.ServiceInterfaces;

namespace QMUL.DiabetesBackend.ServiceImpl.Implementations
{
    public class ServiceRequestService : IServiceRequestService
    {
        private readonly IServiceRequestDao serviceRequestDao;
        private readonly IPatientDao patientDao;
        private readonly IEventDao eventDao;

        public ServiceRequestService(IServiceRequestDao serviceRequestDao, IPatientDao patientDao, IEventDao eventDao)
        {
            this.serviceRequestDao = serviceRequestDao;
            this.patientDao = patientDao;
            this.eventDao = eventDao;
        }

        public async Task<ServiceRequest> CreateServiceRequest(ServiceRequest request)
        {
            var serviceRequest = await this.serviceRequestDao.CreateServiceRequest(request);
            var patient = await this.patientDao.GetPatientByIdOrEmail(serviceRequest.Subject.ElementId);
            var events = EventsGenerator.GenerateEventsFrom(serviceRequest, patient);
            var eventsResult = await this.eventDao.CreateEvents(events);
            if (!eventsResult)
            {
                throw new ArgumentException($"Unable to create events related to the request: {serviceRequest.Id}");
            }

            return serviceRequest;
        }

        public Task<ServiceRequest> GetServiceRequest(string id)
        {
            return this.serviceRequestDao.GetServiceRequest(id);
        }

        public Task<ServiceRequest> UpdateServiceRequest(string id, ServiceRequest request)
        {
            var exists = this.serviceRequestDao.GetServiceRequest(id) != null;
            if (exists)
            {
                return this.serviceRequestDao.UpdateServiceRequest(id, request);
            }

            throw new KeyNotFoundException();
        }

        public Task<bool> DeleteServiceRequest(string id)
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