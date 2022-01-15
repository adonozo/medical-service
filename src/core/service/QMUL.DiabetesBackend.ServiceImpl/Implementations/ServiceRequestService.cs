namespace QMUL.DiabetesBackend.ServiceImpl.Implementations
{
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
            var patient = await ExceptionHandler.ExecuteAndHandleAsync(async () =>
                await this.patientDao.GetPatientByIdOrEmail(request.Subject.ElementId), this.logger);
            var serviceRequest = await ExceptionHandler.ExecuteAndHandleAsync(async () =>
                await this.serviceRequestDao.CreateServiceRequest(request), this.logger);
            var events = ResourceUtils.GenerateEventsFrom(serviceRequest, patient);
            await ExceptionHandler.ExecuteAndHandleAsync(async () => await this.eventDao.CreateEvents(events),
                this.logger);
            this.logger.LogDebug("Service Request created with ID: {Id}", serviceRequest.Id);
            return serviceRequest;
        }

        /// <inheritdoc/>>
        public async Task<ServiceRequest> GetServiceRequest(string id)
        {
            var serviceRequest = await ExceptionHandler.ExecuteAndHandleAsync(async () =>
                await this.serviceRequestDao.GetServiceRequest(id), this.logger);
            this.logger.LogDebug("Found service request {Id}", id);
            return serviceRequest;
        }

        /// <inheritdoc/>>
        public async Task<ServiceRequest> UpdateServiceRequest(string id, ServiceRequest request)
        {
            await ExceptionHandler.ExecuteAndHandleAsync(async () =>
                await this.serviceRequestDao.GetServiceRequest(id), this.logger);
            this.logger.LogDebug("Service request with ID {Id} updated", id);
            return await ExceptionHandler.ExecuteAndHandleAsync(async () =>
                await this.serviceRequestDao.UpdateServiceRequest(id, request), this.logger);
        }

        /// <inheritdoc/>>
        public async Task<bool> DeleteServiceRequest(string id)
        {
            // Check if the service request exists
            await ExceptionHandler.ExecuteAndHandleAsync(async () =>
                await this.serviceRequestDao.GetServiceRequest(id), this.logger);
            this.logger.LogDebug("Service request {Id} deleted", id);
            return await this.serviceRequestDao.DeleteServiceRequest(id);
        }
    }
}