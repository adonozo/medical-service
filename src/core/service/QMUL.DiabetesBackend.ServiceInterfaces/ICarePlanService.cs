namespace QMUL.DiabetesBackend.ServiceInterfaces
{
    using System.Threading.Tasks;
    using Hl7.Fhir.Model;
    
    /// <summary>
    /// The Care Plan Service Interface
    /// </summary>
    public interface ICarePlanService
    {
        public Task<Bundle> GetActiveCarePlans(string patientIdOrEmail);

        public Task<Bundle> GetCarePlanFor(string patientId);
    }
}