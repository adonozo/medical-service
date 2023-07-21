namespace QMUL.DiabetesBackend.Service;

using System;
using System.Linq;
using System.Threading.Tasks;
using DataInterfaces;
using Hl7.Fhir.Model;
using Microsoft.Extensions.Logging;
using Model;
using Model.Exceptions;
using Model.Extensions;
using ServiceInterfaces;
using ServiceInterfaces.Utils;
using Utils;
using ResourceReference = Hl7.Fhir.Model.ResourceReference;
using Task = System.Threading.Tasks.Task;

/// <summary>
/// Manages Medication Requests
/// </summary>
public class MedicationRequestService : IMedicationRequestService
{
    private readonly IMedicationRequestDao medicationRequestDao;
    private readonly IMedicationDao medicationDao;
    private readonly IEventDao eventDao;
    private readonly IPatientDao patientDao;
    private readonly IDataGatherer dataGatherer;
    private readonly ILogger<MedicationRequestService> logger;

    public MedicationRequestService(
        IMedicationRequestDao medicationRequestDao,
        IEventDao eventDao,
        IPatientDao patientDao,
        IMedicationDao medicationDao,
        IDataGatherer dataGatherer,
        ILogger<MedicationRequestService> logger)
    {
        this.medicationRequestDao = medicationRequestDao;
        this.eventDao = eventDao;
        this.patientDao = patientDao;
        this.medicationDao = medicationDao;
        this.dataGatherer = dataGatherer;
        this.logger = logger;
    }

    /// <inheritdoc/>>
    public async Task<MedicationRequest> CreateMedicationRequest(MedicationRequest request)
    {
        await this.dataGatherer.GetReferencePatientOrThrow(request.Subject);
        await this.SetInsulinRequest(request);
        request.AuthoredOn = DateTime.UtcNow.ToString("O");

        foreach (var dosage in request.DosageInstruction.Where(dosage => dosage.Timing.Repeat.NeedsStartDate()))
        {
            dosage.Timing.SetNeedsStartDateFlag();
        }

        var newRequest = await this.medicationRequestDao.CreateMedicationRequest(request);
        this.logger.LogDebug("Medication request created with ID {Id}", newRequest.Id);
        return newRequest;
    }

    /// <inheritdoc/>>
    public Task<MedicationRequest?> GetMedicationRequest(string id)
    {
        return this.medicationRequestDao.GetMedicationRequest(id);
    }

    /// <inheritdoc/>>
    public async Task<bool> UpdateMedicationRequest(string id, MedicationRequest request)
    {
        await ResourceUtils.GetResourceOrThrowAsync(() => this.medicationRequestDao.GetMedicationRequest(id),
            new NotFoundException());
        await this.SetInsulinRequest(request);

        if (await this.dataGatherer.ResourceHasActiveCarePlan(request))
        {
            throw new ValidationException($"Medication request {id} is part of an active care plan");
        }

        var internalPatient = await this.dataGatherer.GetReferenceInternalPatientOrThrow(request.Subject);
        request.Id = id;
        var updateResult = await this.medicationRequestDao.UpdateMedicationRequest(id, request);
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
    public async Task<bool> DeleteMedicationRequest(string id)
    {
        var medicationRequest = await ResourceUtils.GetResourceOrThrowAsync(() => this.medicationRequestDao.GetMedicationRequest(id),
            new NotFoundException());
        
        if (await this.dataGatherer.ResourceHasActiveCarePlan(medicationRequest))
        {
            throw new ValidationException($"Medication request {id} is part of an active care plan");
        }

        await this.eventDao.DeleteRelatedEvents(id);
        return await this.medicationRequestDao.DeleteMedicationRequest(id);
    }

    /// <inheritdoc/>>
    public async Task<PaginatedResult<Bundle>> GetActiveMedicationRequests(string patientIdOrEmail,
        PaginationRequest paginationRequest)
    {
        var patient = await ResourceUtils.GetResourceOrThrowAsync(() =>
            this.patientDao.GetPatientByIdOrEmail(patientIdOrEmail), new NotFoundException());

        var medicationRequests =
            await this.medicationRequestDao.GetActiveMedicationRequests(patient.Id, paginationRequest, false);

        var paginatedBundle = medicationRequests.ToBundleResult();
        this.logger.LogDebug("Found {Count} active medication requests", medicationRequests.Results.Count());
        return paginatedBundle;
    }

    /// <summary>
    /// Checks if the <see cref="MedicationRequest"/> has am insulin type <see cref="Medication"/>. If this is true,
    /// it adds the insulin flag extension to the medication request.
    /// The medication can be contained as part of the request, or be just a reference with the medication ID.
    /// </summary>
    /// <param name="request">The <see cref="MedicationRequest"/>.</param>
    public async Task SetInsulinRequest(MedicationRequest request)
    {
        var medicationReference = request.Medication;
        if (medicationReference is not ResourceReference reference || string.IsNullOrWhiteSpace(reference.Reference))
        {
            return;
        }

        var resource = request.FindContainedResource(reference.Reference);
        Medication? medication;
        if (resource is not Medication)
        {
            var medicationId = reference.GetIdFromReference();
            medication = await this.medicationDao.GetSingleMedication(medicationId);
        }
        else
        {
            medication = resource as Medication;
        }

        if (medication != null && medication.HasInsulinFlag())
        {
            request.SetInsulinFlag();
        }
    }
}