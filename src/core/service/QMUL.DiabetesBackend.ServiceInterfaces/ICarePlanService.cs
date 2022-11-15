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
        /// <exception cref="NotFoundException">If the patient is not found</exception>
        public Task<Bundle> GetActiveCarePlans(string patientIdOrEmail);

        /// <summary>
        /// Gets all the medication requests and service requests for a given patient.
        /// </summary>
        /// <param name="patientIdOrEmail">The patient ID.</param>
        /// <returns>A <see cref="Bundle"/> with all medication and service requests for the patient.</returns>
        /// <exception cref="NotFoundException">If the patient is not found</exception>
        public Task<Bundle> GetCarePlanFor(string patientIdOrEmail);
    }
}