namespace QMUL.DiabetesBackend.Service;

using System;
using System.Threading.Tasks;
using DataInterfaces;
using Hl7.Fhir.Model;
using Microsoft.Extensions.Logging;
using Model.Exceptions;
using Model.Extensions;
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
        var patientId = request.Subject.GetIdFromReference();
        var patientNotFoundException = new ValidationException($"Patient not found: {patientId}");
        var patient = await ResourceUtils.GetResourceOrThrowAsync(async () =>
            await this.patientDao.GetPatientByIdOrEmail(patientId), patientNotFoundException);
        var internalPatient = patient.ToInternalPatient();

        request.AuthoredOn = DateTime.UtcNow.ToString("O");
        var serviceRequest = await this.serviceRequestDao.CreateServiceRequest(request);

        var events = ResourceUtils.GenerateEventsFrom(serviceRequest, internalPatient);
        await this.eventDao.CreateEvents(events);
        this.logger.LogDebug("Service Request created with ID: {Id}", serviceRequest.Id);
        return serviceRequest;
    }

    /// <inheritdoc/>>
    public Task<ServiceRequest?> GetServiceRequest(string id)
    {
        return this.serviceRequestDao.GetServiceRequest(id);
    }

    /// <inheritdoc/>>
    public async Task<bool> UpdateServiceRequest(string id, ServiceRequest request)
    {
        var serviceNotFoundException = new NotFoundException();
        await ResourceUtils.GetResourceOrThrowAsync(async () =>
            await this.serviceRequestDao.GetServiceRequest(id), serviceNotFoundException);

        var patientId = request.Subject.GetIdFromReference();
        var patientNotFoundException = new ValidationException($"Patient not found: {patientId}");
        var patient = await ResourceUtils.GetResourceOrThrowAsync(async () =>
            await this.patientDao.GetPatientByIdOrEmail(patientId), patientNotFoundException);
        var internalPatient = patient.ToInternalPatient();

        request.Id = id;
        var result = await this.serviceRequestDao.UpdateServiceRequest(id, request);

        if (!result)
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
        var serviceNotFoundException = new NotFoundException();
        await ResourceUtils.GetResourceOrThrowAsync(async () =>
            await this.serviceRequestDao.GetServiceRequest(id), serviceNotFoundException);

        await this.eventDao.DeleteRelatedEvents(id);
        return await this.serviceRequestDao.DeleteServiceRequest(id);
    }
}