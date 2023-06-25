namespace QMUL.DiabetesBackend.ServiceInterfaces;

using System.Threading.Tasks;
using Hl7.Fhir.Model;
using Model;
using Model.Exceptions;

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
    /// Gets all the <see cref="CarePlan"/> for a given patient in a <see cref="PaginatedResult{T}"/> object
    /// </summary>
    /// <param name="patientIdOrEmail">The patient's Id or email.</param>
    /// <param name="paginationRequest">The <see cref="PaginationRequest"/> object used for pagination</param>
    /// <returns>A <see cref="PaginatedResult{T}"/> with the patient's <see cref="CarePlan"/> as entries.</returns>
    Task<PaginatedResult<Bundle>> GetCarePlansFor(string patientIdOrEmail, PaginationRequest paginationRequest);

    /// <summary>
    /// Creates a single <see cref="CarePlan"/>
    /// </summary>
    /// <param name="carePlan">The <see cref="CarePlan"/> to create</param>
    /// <returns>The created <see cref="CarePlan"/></returns>
    Task<CarePlan> CreateCarePlan(CarePlan carePlan);

    /// <summary>
    /// Add a <see cref="ServiceRequest"/> into a <see cref="CarePlan"/> in 'Draft' status 
    /// </summary>
    /// <param name="carePlanId">The care plan ID to attach the service request</param>
    /// <param name="request">The <see cref="ServiceRequest"/> to add to the care plan</param>
    /// <returns>A boolean value to indicate if the operation was successful</returns>
    /// <exception cref="ValidationException">If the care plan is not in 'Draft' status</exception>
    Task<bool> AddServiceRequest(string carePlanId, ServiceRequest request);

    /// <summary>
    /// Add a <see cref="MedicationRequest"/> into a <see cref="CarePlan"/> in 'Draft' status
    /// </summary>
    /// <param name="carePlanId">The care plan ID to attach the service request</param>
    /// <param name="request">The <see cref="MedicationRequest"/> to add to the care plan</param>
    /// <returns>A boolean value to indicate if the operation was successful</returns>
    /// <exception cref="ValidationException">If the care plan is not in 'Draft' status</exception>
    Task<bool> AddMedicationRequest(string carePlanId, MedicationRequest request);

    /// <summary>
    /// Changes the status of a care plan to 'Active'
    /// </summary>
    /// <param name="id">The care plan ID to activate</param>
    /// <returns>A boolean value to indicate if the operation was successful</returns>
    /// <exception cref="ValidationException">If the care plan is not in 'Draft' status</exception>
    /// <exception cref="NotFoundException">If the care plan was not found</exception>
    Task<bool> ActivateCarePlan(string id);

    /// <summary>
    /// Changes the status of a care plan to 'Revoked'
    /// </summary>
    /// <param name="id">The care plan ID to revoke</param>
    /// <returns>A boolean value to indicate if the operation was successful</returns>
    /// <exception cref="ValidationException">If the care plan is 'Completed' status</exception>
    /// <exception cref="NotFoundException">If the care plan was not found</exception>
    Task<bool> RevokeCarePlan(string id);

    /// <summary>
    /// Deletes a single care plan
    /// </summary>
    /// <param name="id">The care plan ID to delete</param>
    /// <returns>A boolean value to indicate if the operation was successful</returns>
    /// <exception cref="NotFoundException">If the care plan was not found</exception>
    Task<bool> DeleteCarePlan(string id);

    /// <summary>
    /// Removes a service request from a care plan
    /// </summary>
    /// <param name="carePlanId">The care plan ID to remove the service request from</param>
    /// <param name="serviceRequestId">The service request ID to remove</param>
    /// <returns>A boolean value to indicate if the operation was successful</returns>
    Task<bool> DeleteServiceRequest(string carePlanId, string serviceRequestId);

    /// <summary>
    /// Removes a medication request from a care plan
    /// </summary>
    /// <param name="carePlanId">The care plan ID to remove the medication request from</param>
    /// <param name="medicationRequestId">The medication request ID to remove</param>
    /// <returns>A boolean value to indicate if the operation was successful</returns>
    Task<bool> DeleteMedicationRequest(string carePlanId, string medicationRequestId);

    /// <summary>
    /// Gets a single <see cref="CarePlan"/>
    /// </summary>
    /// <param name="id">The ID to search</param>
    /// <returns>A <see cref="CarePlan"/> or null if the care plan does not exist</returns>
    Task<CarePlan?> GetCarePlan(string id);

    /// <summary>
    /// Gets a <see cref="CarePlan"/> and all the contained <see cref="MedicationRequest"/> and <see cref="ServiceRequest"/>
    /// </summary>
    /// <param name="id">The care plan ID</param>
    /// <returns>A <see cref="Bundle"/> object with all of the care plan objects, or null if the care plan does not exist</returns>
    Task<Bundle?> GetDetailedCarePan(string id);
}