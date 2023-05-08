namespace QMUL.DiabetesBackend.Service;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataInterfaces;
using Hl7.Fhir.Model;
using Microsoft.Extensions.Logging;
using Model;
using Model.Exceptions;
using Model.Extensions;
using ServiceInterfaces;
using Utils;
using ResourceReference = Hl7.Fhir.Model.ResourceReference;

/// <summary>
/// The care plan service handles care plans which are the set of medication and service requests for a patients
/// (a treatment). 
/// </summary>
public class CarePlanService : ICarePlanService
{
    private readonly IMedicationRequestDao medicationRequestDao;
    private readonly IServiceRequestDao serviceRequestDao;
    private readonly IPatientDao patientDao;
    private readonly ICarePlanDao carePlanDao;
    private readonly IEventDao eventDao;
    private readonly IServiceRequestService serviceRequestService;
    private readonly IMedicationRequestService medicationRequestService;
    private readonly ILogger<CarePlanService> logger;

    public CarePlanService(IServiceRequestDao serviceRequestDao,
        IMedicationRequestDao medicationRequestDao,
        IPatientDao patientDao,
        ICarePlanDao carePlanDao,
        IEventDao eventDao,
        IServiceRequestService serviceRequestService,
        IMedicationRequestService medicationRequestService,
        ILogger<CarePlanService> logger)
    {
        this.serviceRequestDao = serviceRequestDao;
        this.medicationRequestDao = medicationRequestDao;
        this.patientDao = patientDao;
        this.carePlanDao = carePlanDao;
        this.logger = logger;
        this.eventDao = eventDao;
        this.serviceRequestService = serviceRequestService;
        this.medicationRequestService = medicationRequestService;
    }

    /// <inheritdoc/>
    public async Task<Bundle?> GetActiveCarePlans(string patientIdOrEmail)
    {
        this.logger.LogTrace("Getting active care plans for {IdOrEmail}", patientIdOrEmail);
        var patient = await this.patientDao.GetPatientByIdOrEmail(patientIdOrEmail);
        if (patient is null)
        {
            return null;
        }

        var medicationRequests = await this.medicationRequestDao.GetAllActiveMedicationRequests(patient.Id);
        var serviceRequests = await this.serviceRequestDao.GetActiveServiceRequests(patient.Id);
        return this.GetSearchBundle(medicationRequests, serviceRequests);
    }

    /// <inheritdoc/>
    public async Task<Bundle?> GetCarePlanFor(string patientIdOrEmail)
    {
        this.logger.LogTrace("Getting care plans for {IdOrEmail}", patientIdOrEmail);
        var patient = await this.patientDao.GetPatientByIdOrEmail(patientIdOrEmail);
        if (patient is null)
        {
            return null;
        }

        var medicationRequests = await this.medicationRequestDao.GetMedicationRequestFor(patient.Id);
        var serviceRequests = await this.serviceRequestDao.GetServiceRequestsFor(patient.Id);
        return this.GetSearchBundle(medicationRequests, serviceRequests);
    }

    public async Task<PaginatedResult<Bundle>> GetCarePlansFor(string patientIdOrEmail, PaginationRequest paginationRequest)
    {
        await ResourceUtils.GetResourceOrThrowAsync(async () =>
            await this.patientDao.GetPatientByIdOrEmail(patientIdOrEmail), new NotFoundException());

        var carePlans = await this.carePlanDao.GetCarePlans(patientIdOrEmail, paginationRequest);
        return carePlans.ToBundleResult();
    }

    public async Task<CarePlan> CreateCarePlan(CarePlan carePlan)
    {
        var patientId = carePlan.Subject.GetIdFromReference();
        var patientException = new ValidationException($"Patient not found: {patientId}");
        await ResourceUtils.GetResourceOrThrowAsync(() =>
            this.patientDao.GetPatientByIdOrEmail(patientId), patientException);

        carePlan.Status = RequestStatus.Draft;
        carePlan.Created = DateTime.UtcNow.ToString("O");
        return await this.carePlanDao.CreateCarePlan(carePlan);
    }

    public async Task<bool> AddServiceRequest(string carePlanId, ServiceRequest request)
    {
        var carePlan = await ResourceUtils.GetResourceOrThrowAsync(() => this.carePlanDao.GetCarePlan(carePlanId),
            new NotFoundException());

        if (carePlan.Status != RequestStatus.Draft)
        {
            throw new ValidationException($"Cannot add a service request to a care plan with status {carePlan.Status}");
        }

        request.Subject.SetPatientReference(carePlan.Subject.GetIdFromReference());
        request.SetCarePlanReference(carePlanId);
        var newRequest = await this.serviceRequestService.CreateServiceRequest(request);

        var activity = new CarePlan.ActivityComponent
        {
            Reference = newRequest.GetReference()
        };

        carePlan.Activity ??= new List<CarePlan.ActivityComponent>();
        carePlan.Activity.Add(activity);

        return await this.carePlanDao.UpdateCarePlan(carePlanId, carePlan);
    }

    public async Task<bool> AddMedicationRequest(string carePlanId, MedicationRequest request)
    {
        var carePlan = await ResourceUtils.GetResourceOrThrowAsync(() => this.carePlanDao.GetCarePlan(carePlanId),
            new NotFoundException());
        
        if (carePlan.Status != RequestStatus.Draft)
        {
            throw new ValidationException($"Cannot add a medication request to a care plan with status {carePlan.Status}");
        }

        request.Subject.SetPatientReference(carePlan.Subject.GetIdFromReference());
        request.SetCarePlanReference(carePlanId);
        var newRequest = await this.medicationRequestService.CreateMedicationRequest(request);

        var activity = new CarePlan.ActivityComponent
        {
            Reference = newRequest.GetReference()
        };

        carePlan.Activity ??= new List<CarePlan.ActivityComponent>();
        carePlan.Activity.Add(activity);

        return await this.carePlanDao.UpdateCarePlan(carePlanId, carePlan);
    }

    public async Task<bool> ActivateCarePlan(string id)
    {
        var carePlan = await ResourceUtils.GetResourceOrThrowAsync(() => this.carePlanDao.GetCarePlan(id),
            new NotFoundException());

        carePlan.Status = RequestStatus.Active;
        return await this.carePlanDao.UpdateCarePlan(id, carePlan);
    }

    public async Task<bool> RevokeCarePlan(string id)
    {
        var carePlan = await ResourceUtils.GetResourceOrThrowAsync(() => this.carePlanDao.GetCarePlan(id),
            new NotFoundException());

        carePlan.Status = RequestStatus.Revoked;
        return await this.carePlanDao.UpdateCarePlan(id, carePlan);
    }

    public async Task<bool> DeleteCarePlan(string id)
    {
        var carePlan = await ResourceUtils.GetResourceOrThrowAsync(() => this.carePlanDao.GetCarePlan(id),
            new NotFoundException());

        var (medicationRequestReferences, serviceRequestReferences) = this.GetReferences(carePlan);
        var medicationRequestsIds = medicationRequestReferences
            .Select(reference => reference.GetIdFromReference())
            .ToArray();
        var serviceRequestIds = serviceRequestReferences
            .Select(reference => reference.GetIdFromReference())
            .ToArray();
        var resourceIds = medicationRequestsIds.Union(serviceRequestIds);

        await this.eventDao.DeleteAllRelatedResources(resourceIds.ToArray());
        await this.medicationRequestDao.DeleteMedicationRequests(medicationRequestsIds);
        await this.serviceRequestDao.DeleteServiceRequests(serviceRequestIds);
        
        return await this.carePlanDao.DeleteCarePlan(id);
    }

    public async Task<bool> DeleteServiceRequest(string carePlanId, string serviceRequestId)
    {
        var carePlan = await ResourceUtils.GetResourceOrThrowAsync(() => this.carePlanDao.GetCarePlan(carePlanId),
            new NotFoundException());

        await this.serviceRequestService.DeleteServiceRequest(serviceRequestId);

        carePlan.Activity ??= new List<CarePlan.ActivityComponent>();
        carePlan.Activity.RemoveAll(activity => activity.Reference.Reference.Contains(serviceRequestId));
        return await this.carePlanDao.UpdateCarePlan(carePlanId, carePlan);
    }

    public async Task<bool> DeleteMedicationRequest(string carePlanId, string medicationRequestId)
    {
        var carePlan = await ResourceUtils.GetResourceOrThrowAsync(() => this.carePlanDao.GetCarePlan(carePlanId),
            new NotFoundException());

        await this.medicationRequestService.DeleteMedicationRequest(medicationRequestId);

        carePlan.Activity ??= new List<CarePlan.ActivityComponent>();
        carePlan.Activity.RemoveAll(activity => activity.Reference.Reference.Contains(medicationRequestId));
        return await this.carePlanDao.UpdateCarePlan(carePlanId, carePlan);
    }

    public Task<CarePlan?> GetCarePlan(string id)
    {
        return this.carePlanDao.GetCarePlan(id);
    }

    public async Task<Bundle?> GetDetailedCarePan(string id)
    {
        var carePlan = await this.carePlanDao.GetCarePlan(id);
        if (carePlan is null)
        {
            return null;
        }

        var (medicationRequestsReferences, serviceRequestsReferences) = this.GetReferences(carePlan);

        var medicationRequestIds = medicationRequestsReferences
            .Select(reference => reference.GetIdFromReference())
            .ToArray();
        var medicationRequests = await this.medicationRequestDao.GetMedicationRequestsByIds(medicationRequestIds);

        var serviceRequestIds = serviceRequestsReferences
            .Select(reference => reference.GetIdFromReference())
            .ToArray();
        var serviceRequests = await this.serviceRequestDao.GetServiceRequestsByIds(serviceRequestIds);
        return this.GetSearchBundle(medicationRequests, serviceRequests);
    }

    private Bundle GetSearchBundle(ICollection<MedicationRequest> medicationRequests,
        ICollection<ServiceRequest> serviceRequests)
    {
        var entries = new List<Resource>(medicationRequests);
        entries.AddRange(serviceRequests);

        this.logger.LogTrace("Found {Count} medication requests", medicationRequests.Count);
        this.logger.LogTrace("Found {Count} service requests", serviceRequests.Count);
        return ResourceUtils.GenerateSearchBundle(entries);
    }

    private (List<ResourceReference> MedicationReferences, List<ResourceReference> ServiceReferences) GetReferences(CarePlan carePlan)
    {
        var medicationRequestsReferences = new List<ResourceReference>();
        var serviceRequestsReferences = new List<ResourceReference>();

        foreach (var activity in carePlan.Activity)
        {
            switch (activity.Reference.Type)
            {
                case nameof(ServiceRequest):
                    serviceRequestsReferences.Add(activity.Reference);
                    break;
                case nameof(MedicationRequest):
                    medicationRequestsReferences.Add(activity.Reference);
                    break;
            }
        }

        return (medicationRequestsReferences, serviceRequestsReferences);
    }
}