namespace QMUL.DiabetesBackend.Service;

using System;
using System.Threading.Tasks;
using DataInterfaces;
using Hl7.Fhir.Model;
using Microsoft.Extensions.Logging;
using Model.Exceptions;
using Model.Extensions;
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
        await this.dataGatherer.GetReferencePatientOrThrow(request.Subject);
        request.AuthoredOn = DateTime.UtcNow.ToString("O");
        request.Status = RequestStatus.Draft;
        if (request.Occurrence is Timing timing && timing.Repeat.NeedsStartDate())
        {
            timing.SetNeedsStartDateFlag();
            request.Occurrence = timing;
        }

        request = await this.serviceRequestDao.CreateServiceRequest(request);
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

        request.Id = id;
        return await this.serviceRequestDao.UpdateServiceRequest(id, request);;
    }

    /// <inheritdoc/>>
    public Task<bool> ActivateServiceRequests(string[] ids)
    {
        return this.serviceRequestDao.UpdateServiceRequestsStatus(ids, RequestStatus.Active);
    }

    /// <inheritdoc/>>
    public Task<bool> RevokeServiceRequests(string[] ids)
    {
        return this.serviceRequestDao.UpdateServiceRequestsStatus(ids, RequestStatus.Revoked);
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