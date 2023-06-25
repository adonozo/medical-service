namespace QMUL.DiabetesBackend.Service.Utils;

using System.Threading.Tasks;
using DataInterfaces;
using FluentValidation;
using Hl7.Fhir.Model;
using Model;
using Model.Extensions;
using ServiceInterfaces.Utils;
using ResourceReference = Hl7.Fhir.Model.ResourceReference;

/// <summary>
/// Utils to get resource related data
/// </summary>
public class DataGatherer : IDataGatherer
{
    private readonly IPatientDao patientDao;
    private readonly ICarePlanDao carePlanDao;

    public DataGatherer(IPatientDao patientDao, ICarePlanDao carePlanDao)
    {
        this.patientDao = patientDao;
        this.carePlanDao = carePlanDao;
    }

    /// <summary>
    /// Gets an <see cref="InternalPatient"/> given a <see cref="ResourceReference"/>
    /// </summary>
    /// <param name="reference">A <see cref="ResourceReference"/> that should reference a patient</param>
    /// <returns>An awaitable <see cref="InternalPatient"/></returns>
    /// <exception cref="ValidationException">If the reference does not contain an ID or if the patient is not found</exception>
    public async Task<InternalPatient> GetReferenceInternalPatientOrThrow(ResourceReference reference)
    {
        var patient = await this.GetReferencePatientOrThrow(reference);
        return patient.ToInternalPatient();
    }

    /// <summary>
    /// Gets a <see cref="Patient"/> from a given <see cref="ResourceReference"/>
    /// </summary>
    /// <param name="reference">A <see cref="ResourceReference"/> that should reference a patient</param>
    /// <returns>An awaitable <see cref="Patient"/></returns>
    /// <exception cref="ValidationException">If the reference does not contain an ID or if the patient is not found</exception>
    public async Task<Patient> GetReferencePatientOrThrow(ResourceReference reference)
    {
        var patientId = reference.GetIdFromReference();
        if (string.IsNullOrEmpty(patientId))
        {
            throw new ValidationException("Subject is not a Patient reference");
        }

        var patientNotFoundException = new ValidationException($"Patient not found: {patientId}");
        return await ResourceUtils.GetResourceOrThrowAsync(async () =>
            await this.patientDao.GetPatientByIdOrEmail(patientId), patientNotFoundException);
    }

    /// <summary>
    /// Checks if a <see cref="DomainResource"/>, such as a medication or service request, is part of an care plan which
    /// is also active 
    /// </summary>
    /// <param name="resource">A <see cref="DomainResource"/> object</param>
    /// <returns>A boolean value, true if the resource belongs to an active care plan, false otherwise</returns>
    public async Task<bool> ResourceHasActiveCarePlan(DomainResource resource)
    {
        var carePlanId = resource.GetCarePlanReference();
        if (carePlanId is null)
        {
            return false;
        }

        var carePlan = await this.carePlanDao.GetCarePlan(carePlanId);
        return carePlan is not null && carePlan.Status == RequestStatus.Active;
    }
}