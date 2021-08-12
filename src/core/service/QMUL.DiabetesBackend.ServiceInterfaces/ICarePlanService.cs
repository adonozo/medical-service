using System.Collections.Generic;
using System.Threading.Tasks;
using Hl7.Fhir.Model;

namespace QMUL.DiabetesBackend.ServiceInterfaces
{
    public interface ICarePlanService
    {
        public CarePlan CreateCarePlan(CarePlan carePlan);

        public CarePlan GetCarePlan(string id);

        public Task<Bundle> GetActiveCarePlans(string patientIdOrEmail);

        public Task<Bundle> GetCarePlanFor(string patientId);

        public CarePlan UpdateCarePlan(string id, CarePlan carePlan);

        public bool DeleteCarePlan(string id);
    }
}