namespace QMUL.DiabetesBackend.Service.Utils;

using System.Threading.Tasks;
using DataInterfaces;
using FluentValidation;
using Hl7.Fhir.Model;
using Model;
using Model.Extensions;
using ServiceInterfaces.Utils;

public class DataGatherer : IDataGatherer
{
    private readonly IPatientDao patientDao;
    private readonly ICarePlanDao carePlanDao;

    public DataGatherer(IPatientDao patientDao, ICarePlanDao carePlanDao)
    {
        this.patientDao = patientDao;
        this.carePlanDao = carePlanDao;
    }

    public async Task<Patient> GetPatientOrThrow(string patientId)
    {
        var patientNotFoundException = new ValidationException($"Patient not found: {patientId}");
        return await ResourceUtils.GetResourceOrThrowAsync(async () =>
            await this.patientDao.GetPatientByIdOrEmail(patientId), patientNotFoundException);
    }

    public async Task<InternalPatient> GetInternalPatientOrThrow(string patientId)
    {
        var patient = await this.GetPatientOrThrow(patientId);
        return patient.ToInternalPatient();
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