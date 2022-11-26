namespace QMUL.DiabetesBackend.Service;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataInterfaces;
using Hl7.Fhir.Model;
using Microsoft.Extensions.Logging;
using Model.Exceptions;
using Model.Extensions;
using ServiceInterfaces;
using Utils;

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

    public async Task<CarePlan> CreateCarePlan(CarePlan carePlan)
    {
        var patientId = carePlan.Subject.GetIdFromReference();
        var patientException = new ValidationException($"Patient not found: {patientId}");
        await ResourceUtils.GetResourceOrThrow(() =>
            this.patientDao.GetPatientByIdOrEmail(patientId), patientException);

        carePlan.Status = RequestStatus.Draft;
        carePlan.Created = DateTime.UtcNow.ToString("O");
        return await this.carePlanDao.CreateCarePlan(carePlan);
    }

    public async Task<bool> AddServiceRequest(string carePlanId, ServiceRequest request)
    {
        var carePlan = await ResourceUtils.GetResourceOrThrow(() => this.carePlanDao.GetCarePlan(carePlanId),
            new NotFoundException());

        request.Subject.SetPatientReference(carePlan.Subject.GetIdFromReference());
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
        var carePlan = await ResourceUtils.GetResourceOrThrow(() => this.carePlanDao.GetCarePlan(carePlanId),
            new NotFoundException());

        request.Subject.SetPatientReference(carePlan.Subject.GetIdFromReference());
        var newRequest = await this.medicationRequestService.CreateMedicationRequest(request);

        var activity = new CarePlan.ActivityComponent
        {
            Reference = newRequest.GetReference()
        };

        carePlan.Activity ??= new List<CarePlan.ActivityComponent>();
        carePlan.Activity.Add(activity);

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

    private Bundle GetSearchBundle(ICollection<MedicationRequest> medicationRequests, ICollection<ServiceRequest> serviceRequests)
    {
        var entries = new List<Resource>(medicationRequests);
        entries.AddRange(serviceRequests);

        this.logger.LogTrace("Found {Count} medication requests", medicationRequests.Count);
        this.logger.LogTrace("Found {Count} service requests", serviceRequests.Count);
        return ResourceUtils.GenerateSearchBundle(entries);
    }
}