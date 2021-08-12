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
            var patient = await ResourceUtils.ValidateObject(
                () => this.patientDao.GetPatientByIdOrEmail(request.Subject.ElementId),
                "Unable to find patient for the Observation", new KeyNotFoundException());
            var serviceRequest = await this.serviceRequestDao.CreateServiceRequest(request);
            var events = EventsGenerator.GenerateEventsFrom(serviceRequest, patient);
            var eventsResult = await this.eventDao.CreateEvents(events);
            if (!eventsResult)
            {
                throw new ArgumentException($"Unable to create events related to the request: {serviceRequest.Id}");
            }

            return serviceRequest;
        }

        public async Task<ServiceRequest> GetServiceRequest(string id)
        {
            return await this.serviceRequestDao.GetServiceRequest(id);
        }

        public async Task<ServiceRequest> UpdateServiceRequest(string id, ServiceRequest request)
        {
            var exists = this.serviceRequestDao.GetServiceRequest(id) != null;
            if (exists)
            {
                return await this.serviceRequestDao.UpdateServiceRequest(id, request);
            }

            throw new KeyNotFoundException();
        }

        public async Task<bool> DeleteServiceRequest(string id)
        {
            var exists = this.serviceRequestDao.GetServiceRequest(id) != null;
            if (exists)
            {
                return await this.serviceRequestDao.DeleteServiceRequest(id);
            }
            
            throw new KeyNotFoundException();
        }
    }
}