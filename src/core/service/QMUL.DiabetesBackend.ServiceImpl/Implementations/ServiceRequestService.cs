namespace QMUL.DiabetesBackend.ServiceImpl.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using DataInterfaces;
    using Hl7.Fhir.Model;
    using ServiceInterfaces;
    using Utils;

    /// <summary>
    /// Manages Services Requests for patients
    /// </summary>
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

        /// <inheritdoc/>>
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

        /// <inheritdoc/>>
        public async Task<ServiceRequest> GetServiceRequest(string id)
        {
            return await this.serviceRequestDao.GetServiceRequest(id);
        }

        /// <inheritdoc/>>
        public async Task<ServiceRequest> UpdateServiceRequest(string id, ServiceRequest request)
        {
            var exists = this.serviceRequestDao.GetServiceRequest(id) != null;
            if (exists)
            {
                return await this.serviceRequestDao.UpdateServiceRequest(id, request);
            }

            throw new KeyNotFoundException();
        }

        /// <inheritdoc/>>
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