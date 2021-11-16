namespace QMUL.DiabetesBackend.ServiceInterfaces
{
    using System.Threading.Tasks;
    using Hl7.Fhir.Model;
    
    /// <summary>
    /// The Care Plan Service Interface
    /// </summary>
    public interface ICarePlanService
    {
        /// <summary>
        /// Gets the active care plans for a patient. Rather than requests status, it gets resources with date intervals
        /// happening at the time of the search. It includes medication and service requests.
        /// </summary>
        /// <param name="patientIdOrEmail">The patient's Id or email.</param>
        /// <returns>The list of medication and service requests as a Bundle.</returns>
        public Task<Bundle> GetActiveCarePlans(string patientIdOrEmail);

        // TODO this gets all care plans for a given patient. Rename the method or repurpose it. (is/will this be used?) 
        public Task<Bundle> GetCarePlanFor(string patientId);
    }
}