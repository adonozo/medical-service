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
    private readonly IServiceRequestService serviceRequestService;
    private readonly IMedicationRequestService medicationRequestService;
    private readonly ILogger<CarePlanService> logger;

    public CarePlanService(IServiceRequestDao serviceRequestDao,
        IMedicationRequestDao medicationRequestDao,
        IPatientDao patientDao,
        ICarePlanDao carePlanDao,
        IServiceRequestService serviceRequestService,
        IMedicationRequestService medicationRequestService,
        ILogger<CarePlanService> logger)
    {
        this.serviceRequestDao = serviceRequestDao;
        this.medicationRequestDao = medicationRequestDao;
        this.patientDao = patientDao;
        this.carePlanDao = carePlanDao;
        this.logger = logger;
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
    public async Task<PaginatedResult<Bundle>> GetCarePlansFor(string patientIdOrEmail,
        PaginationRequest paginationRequest)
    {
        await ResourceUtils.GetResourceOrThrowAsync(async () =>
            await this.patientDao.GetPatientByIdOrEmail(patientIdOrEmail), new NotFoundException());

        var carePlans = await this.carePlanDao.GetCarePlans(patientIdOrEmail, paginationRequest);
        return carePlans.ToBundleResult();
    }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
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
            PlannedActivityReference = newRequest.CreateReference()
        };

        carePlan.Activity ??= new List<CarePlan.ActivityComponent>();
        carePlan.Activity.Add(activity);

        return await this.carePlanDao.UpdateCarePlan(carePlanId, carePlan);
    }

    /// <inheritdoc/>
    public async Task<bool> AddMedicationRequest(string carePlanId, MedicationRequest request)
    {
        var carePlan = await ResourceUtils.GetResourceOrThrowAsync(() => this.carePlanDao.GetCarePlan(carePlanId),
            new NotFoundException());

        if (carePlan.Status != RequestStatus.Draft)
        {
            throw new ValidationException(
                $"Cannot add a medication request to a care plan with status {carePlan.Status}");
        }

        request.Subject.SetPatientReference(carePlan.Subject.GetIdFromReference());
        request.SetCarePlanReference(carePlanId);
        var newRequest = await this.medicationRequestService.CreateMedicationRequest(request);

        var activity = new CarePlan.ActivityComponent
        {
            PlannedActivityReference = newRequest.CreateReference()
        };

        carePlan.Activity ??= new List<CarePlan.ActivityComponent>();
        carePlan.Activity.Add(activity);

        return await this.carePlanDao.UpdateCarePlan(carePlanId, carePlan);
    }

    /// <inheritdoc/>
    public async Task<bool> ActivateCarePlan(string id)
    {
        var carePlan = await ResourceUtils.GetResourceOrThrowAsync(() => this.carePlanDao.GetCarePlan(id),
            new NotFoundException());

        if (carePlan.Status != RequestStatus.Draft)
        {
            throw new ValidationException($"Care plan with ID {id} is not in {RequestStatus.Draft} status");
        }

        carePlan.Status = RequestStatus.Active;
        await this.serviceRequestService.ActivateServiceRequests(carePlan.GetServiceRequestsIds());
        await this.medicationRequestService.ActivateMedicationRequestsStatus(carePlan.GetMedicationRequestsIds());

        return await this.carePlanDao.UpdateCarePlan(id, carePlan);
    }

    /// <inheritdoc/>
    public async Task<bool> RevokeCarePlan(string id)
    {
        var carePlan = await ResourceUtils.GetResourceOrThrowAsync(() => this.carePlanDao.GetCarePlan(id),
            new NotFoundException());

        if (carePlan.Status == RequestStatus.Completed)
        {
            throw new ValidationException($"Care plan with ID {id} is not in {RequestStatus.Completed} status");
        }

        carePlan.Status = RequestStatus.Revoked;
        await this.serviceRequestService.RevokeServiceRequests(carePlan.GetServiceRequestsIds());
        await this.medicationRequestService.RevokeMedicationRequestsStatus(carePlan.GetMedicationRequestsIds());

        return await this.carePlanDao.UpdateCarePlan(id, carePlan);
    }

    /// <inheritdoc/>
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

        await this.medicationRequestDao.DeleteMedicationRequests(medicationRequestsIds);
        await this.serviceRequestDao.DeleteServiceRequests(serviceRequestIds);

        return await this.carePlanDao.DeleteCarePlan(id);
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteServiceRequest(string carePlanId, string serviceRequestId)
    {
        var carePlan = await ResourceUtils.GetResourceOrThrowAsync(() => this.carePlanDao.GetCarePlan(carePlanId),
            new NotFoundException());

        await this.serviceRequestService.DeleteServiceRequest(serviceRequestId);

        carePlan.Activity ??= new List<CarePlan.ActivityComponent>();
        carePlan.Activity.RemoveAll(activity => activity.PlannedActivityReference.Reference.Contains(serviceRequestId));
        return await this.carePlanDao.UpdateCarePlan(carePlanId, carePlan);
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteMedicationRequest(string carePlanId, string medicationRequestId)
    {
        var carePlan = await ResourceUtils.GetResourceOrThrowAsync(() => this.carePlanDao.GetCarePlan(carePlanId),
            new NotFoundException());

        await this.medicationRequestService.DeleteMedicationRequest(medicationRequestId);

        carePlan.Activity ??= new List<CarePlan.ActivityComponent>();
        carePlan.Activity.RemoveAll(activity => activity.PlannedActivityReference.Reference.Contains(medicationRequestId));
        return await this.carePlanDao.UpdateCarePlan(carePlanId, carePlan);
    }

    /// <inheritdoc/>
    public Task<CarePlan?> GetCarePlan(string id)
    {
        return this.carePlanDao.GetCarePlan(id);
    }

    /// <inheritdoc/>
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
        return this.GetSearchBundle(medicationRequests, serviceRequests, carePlan);
    }

    private Bundle GetSearchBundle(ICollection<MedicationRequest> medicationRequests,
        ICollection<ServiceRequest> serviceRequests, CarePlan? carePlan = null)
    {
        var entries = new List<Resource>(medicationRequests);
        entries.AddRange(serviceRequests);
        if (carePlan is not null)
        {
            entries.Add(carePlan);
        }

        this.logger.LogTrace("Found {Count} medication requests", medicationRequests.Count);
        this.logger.LogTrace("Found {Count} service requests", serviceRequests.Count);
        return ResourceUtils.GenerateSearchBundle(entries);
    }

    private (List<ResourceReference> MedicationReferences, List<ResourceReference> ServiceReferences) GetReferences(
        CarePlan carePlan)
    {
        var medicationRequestsReferences = new List<ResourceReference>();
        var serviceRequestsReferences = new List<ResourceReference>();

        foreach (var activity in carePlan.Activity)
        {
            switch (activity.PlannedActivityReference.Type)
            {
                case nameof(ServiceRequest):
                    serviceRequestsReferences.Add(activity.PlannedActivityReference);
                    break;
                case nameof(MedicationRequest):
                    medicationRequestsReferences.Add(activity.PlannedActivityReference);
                    break;
            }
        }

        return (medicationRequestsReferences, serviceRequestsReferences);
    }
}