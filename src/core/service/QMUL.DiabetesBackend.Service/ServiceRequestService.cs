namespace QMUL.DiabetesBackend.Service;

using System;
using System.Threading.Tasks;
using DataInterfaces;
using Hl7.Fhir.Model;
using Microsoft.Extensions.Logging;
using Model.Exceptions;
using ServiceInterfaces;
using ServiceInterfaces.Utils;
using Utils;

/// <summary>
/// Manages Services Requests for patients
/// </summary>
public class ServiceRequestService : IServiceRequestService
{
    private readonly IServiceRequestDao serviceRequestDao;
    private readonly IEventDao eventDao;
    private readonly IDataGatherer dataGatherer;
    private readonly ILogger<ServiceRequestService> logger;

    public ServiceRequestService(
        IServiceRequestDao serviceRequestDao,
        IEventDao eventDao,
        IDataGatherer dataGatherer,
        ILogger<ServiceRequestService> logger)
    {
        this.serviceRequestDao = serviceRequestDao;
        this.eventDao = eventDao;
        this.dataGatherer = dataGatherer;
        this.logger = logger;
    }

    /// <inheritdoc/>>
    public async Task<ServiceRequest> CreateServiceRequest(ServiceRequest request)
    {
        var internalPatient = await this.dataGatherer.GetReferenceInternalPatientOrThrow(request.Subject);
        request.AuthoredOn = DateTime.UtcNow.ToString("O");
        request = await this.serviceRequestDao.CreateServiceRequest(request);

        var events = ResourceUtils.GenerateEventsFrom(request, internalPatient);
        await this.eventDao.CreateEvents(events);
        this.logger.LogDebug("Service Request created with ID: {Id}", request.Id);
        return request;
    }

    /// <inheritdoc/>>
    public Task<ServiceRequest?> GetServiceRequest(string id)
    {
        return this.serviceRequestDao.GetServiceRequest(id);
    }

    /// <inheritdoc/>>
    public async Task<bool> UpdateServiceRequest(string id, ServiceRequest request)
    {
        await ResourceUtils.GetResourceOrThrowAsync(async () =>
            await this.serviceRequestDao.GetServiceRequest(id), new NotFoundException());

        if (await this.dataGatherer.ResourceHasActiveCarePlan(request))
        {
            throw new ValidationException($"Service request {id} is part of an active care plan");
        }

        var internalPatient = await this.dataGatherer.GetReferenceInternalPatientOrThrow(request.Subject);
        request.Id = id;
        var updateResult = await this.serviceRequestDao.UpdateServiceRequest(id, request);
        if (!updateResult)
        {
            return false;
        }

        await this.eventDao.DeleteRelatedEvents(id);
        var events = ResourceUtils.GenerateEventsFrom(request, internalPatient);
        await this.eventDao.CreateEvents(events);

        return true;
    }

    /// <inheritdoc/>>
    public async Task<bool> DeleteServiceRequest(string id)
    {
        var serviceRequest = await ResourceUtils.GetResourceOrThrowAsync(async () =>
            await this.serviceRequestDao.GetServiceRequest(id), new NotFoundException());
        
        if (await this.dataGatherer.ResourceHasActiveCarePlan(serviceRequest))
        {
            throw new ValidationException($"Service request {id} is part of an active care plan");
        }

        await this.eventDao.DeleteRelatedEvents(id);
        return await this.serviceRequestDao.DeleteServiceRequest(id);
    }
}