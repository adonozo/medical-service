namespace QMUL.DiabetesBackend.DataInterfaces;

using System.Threading.Tasks;
using Hl7.Fhir.Model;

public interface ICarePlanDao
{
    Task<CarePlan> CreateCarePlan(CarePlan carePlan);

    Task<CarePlan?> GetCarePlan(string id);

    Task<bool> UpdateCarePlan(string id, CarePlan carePlan);
}