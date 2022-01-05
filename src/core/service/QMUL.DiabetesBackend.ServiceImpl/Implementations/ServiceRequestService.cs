namespace QMUL.DiabetesBackend.ServiceImpl.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using DataInterfaces;
    using Hl7.Fhir.Model;
    using Microsoft.Extensions.Logging;
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
        private readonly ILogger<ServiceRequestService> logger;

        public ServiceRequestService(IServiceRequestDao serviceRequestDao, IPatientDao patientDao, IEventDao eventDao, 
            ILogger<ServiceRequestService> logger)
        {
            this.serviceRequestDao = serviceRequestDao;
            this.patientDao = patientDao;
            this.eventDao = eventDao;
            this.logger = logger;
        }

        /// <inheritdoc/>>
        public async Task<ServiceRequest> CreateServiceRequest(ServiceRequest request)
        {
            var patient = await ResourceUtils.ValidateNullObject(
                () => this.patientDao.GetPatientByIdOrEmail(request.Subject.ElementId),
                new KeyNotFoundException($"Unable to find the patient with ID {request.Subject.ElementId}"));
            var serviceRequest = await this.serviceRequestDao.CreateServiceRequest(request);
            var events = ResourceUtils.GenerateEventsFrom(serviceRequest, patient);
            await ResourceUtils.ValidateBooleanResult(
                () => this.eventDao.CreateEvents(events),
                new ArgumentException($"Unable to create events related to the request: {serviceRequest.Id}"));

            this.logger.LogDebug("Service Request created with ID: {Id}", serviceRequest.Id);
            return serviceRequest;
        }

        /// <inheritdoc/>>
        public async Task<ServiceRequest> GetServiceRequest(string id)
        {
            var serviceRequest = await ResourceUtils.ValidateNullObject(
                () => this.serviceRequestDao.GetServiceRequest(id),
                new KeyNotFoundException($"Unable to find the service request with ID {id}"));
            this.logger.LogDebug("Found service request {Id}", id);
            return serviceRequest;
        }

        /// <inheritdoc/>>
        public async Task<ServiceRequest> UpdateServiceRequest(string id, ServiceRequest request)
        {
            await ResourceUtils.ValidateNullObject(
                () => this.serviceRequestDao.GetServiceRequest(id),
                new KeyNotFoundException($"Service request to be updated not found {id}"));

            this.logger.LogDebug("Service request with ID {Id} updated", id);
            return await this.serviceRequestDao.UpdateServiceRequest(id, request);
        }

        /// <inheritdoc/>>
        public async Task<bool> DeleteServiceRequest(string id)
        {
            await ResourceUtils.ValidateNullObject(
                () => this.serviceRequestDao.GetServiceRequest(id),
                new KeyNotFoundException($"Service request to be updated not found {id}"));

            this.logger.LogDebug("Service request {Id} deleted", id);
            return await this.serviceRequestDao.DeleteServiceRequest(id);
        }
    }
}