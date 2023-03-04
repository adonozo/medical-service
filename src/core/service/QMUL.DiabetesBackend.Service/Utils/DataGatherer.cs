namespace QMUL.DiabetesBackend.Service.Utils;

using System.Threading.Tasks;
using DataInterfaces;
using FluentValidation;
using Hl7.Fhir.Model;
using Model;
using Model.Extensions;
using ServiceInterfaces.Utils;
using ResourceReference = Hl7.Fhir.Model.ResourceReference;

public class DataGatherer : IDataGatherer
{
    private readonly IPatientDao patientDao;
    private readonly ICarePlanDao carePlanDao;

    public DataGatherer(IPatientDao patientDao, ICarePlanDao carePlanDao)
    {
        this.patientDao = patientDao;
        this.carePlanDao = carePlanDao;
    }

    public async Task<InternalPatient> GetReferenceInternalPatientOrThrow(ResourceReference reference)
    {
        var patient = await this.GetReferencePatientOrThrow(reference);
        return patient.ToInternalPatient();
    }

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