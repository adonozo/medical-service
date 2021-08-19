using System.Threading.Tasks;
using Hl7.Fhir.Model;

namespace QMUL.DiabetesBackend.ServiceInterfaces
{
    public interface ICarePlanService
    {
        public Task<Bundle> GetActiveCarePlans(string patientIdOrEmail);

        public Task<Bundle> GetCarePlanFor(string patientId);
    }
}