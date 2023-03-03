namespace QMUL.DiabetesBackend.ServiceInterfaces;

using System.Threading.Tasks;
using Hl7.Fhir.Model;
using Model;

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
    /// <returns>The list of medication and service requests as a Bundle, or null if the patient was not found.</returns>
    Task<Bundle?> GetActiveCarePlans(string patientIdOrEmail);

    /// <summary>
    /// Gets all the medication requests and service requests for a given patient.
    /// </summary>
    /// <param name="patientIdOrEmail">The patient ID.</param>
    /// <returns>A <see cref="Bundle"/> with all medication and service requests for the patient, or null if the patient was not found.</returns>
    Task<Bundle?> GetCarePlanFor(string patientIdOrEmail);

    Task<PaginatedResult<Bundle>> GetCarePlansFor(string patientIdOrEmail, PaginationRequest paginationRequest);

    Task<CarePlan> CreateCarePlan(CarePlan carePlan);

    Task<bool> AddServiceRequest(string carePlanId, ServiceRequest request);

    Task<bool> AddMedicationRequest(string carePlanId, MedicationRequest request);

    Task<bool> ActivateCarePlan(string carePlanId);

    Task<bool> DeleteCarePlan(string id);

    Task<bool> DeleteServiceRequest(string carePlanId, string serviceRequestId);

    Task<bool> DeleteMedicationRequest(string carePlanId, string medicationRequestId);

    Task<CarePlan?> GetCarePlan(string id);

    Task<Bundle?> GetDetailedCarePan(string id);
}