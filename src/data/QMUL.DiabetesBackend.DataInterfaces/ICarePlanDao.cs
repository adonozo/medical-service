namespace QMUL.DiabetesBackend.DataInterfaces;

using System.Collections.Generic;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using Model;

public interface ICarePlanDao
{
    Task<CarePlan> CreateCarePlan(CarePlan carePlan);

    Task<PaginatedResult<IEnumerable<Resource>>> GetCarePlans(string patientId, PaginationRequest paginationRequest);

    Task<CarePlan?> GetCarePlan(string id);

    Task<bool> UpdateCarePlan(string id, CarePlan carePlan);
}