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
        var patientId = request.Subject.GetPatientIdFromReference();
        var patientNotFoundException = new ValidationException($"Patient not found: {patientId}");
        var patient = await ResourceUtils.GetResourceOrThrow(async () =>
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
    public async Task<ServiceRequest> UpdateServiceRequest(string id, ServiceRequest request)
    {
        var serviceNotFoundException = new NotFoundException();
        await ResourceUtils.GetResourceOrThrow(async () =>
            await this.serviceRequestDao.GetServiceRequest(id), serviceNotFoundException);

        var patientId = request.Subject.GetPatientIdFromReference();
        var patientNotFoundException = new ValidationException($"Patient not found: {patientId}");
        var patient = await ResourceUtils.GetResourceOrThrow(async () =>
            await this.patientDao.GetPatientByIdOrEmail(patientId), patientNotFoundException);
        var internalPatient = patient.ToInternalPatient();

        request.Id = id;
        var result = await this.serviceRequestDao.UpdateServiceRequest(id, request);

        await this.eventDao.DeleteRelatedEvents(id);
        var events = ResourceUtils.GenerateEventsFrom(result, internalPatient);
        await this.eventDao.CreateEvents(events);

        return result;
    }

    /// <inheritdoc/>>
    public async Task<bool> DeleteServiceRequest(string id)
    {
        var serviceNotFoundException = new NotFoundException();
        await ResourceUtils.GetResourceOrThrow(async () =>
            await this.serviceRequestDao.GetServiceRequest(id), serviceNotFoundException);

        await this.eventDao.DeleteRelatedEvents(id);
        return await this.serviceRequestDao.DeleteServiceRequest(id);
    }
}