namespace QMUL.DiabetesBackend.DataInterfaces;

using System.Collections.Generic;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using Model;

public interface ICarePlanDao
{
    /// <summary>
    /// Creates a single Care Plan
    /// </summary>
    /// <param name="carePlan">The <see cref="CarePlan"/> to insert into the database</param>
    /// <returns>The created Care Plan</returns>
    Task<CarePlan> CreateCarePlan(CarePlan carePlan);

    /// <summary>
    /// Searches for Care Plans for a patient
    /// </summary>
    /// <param name="patientId">The patient ID for whom the Care Plans are for</param>
    /// <param name="paginationRequest">The <see cref="PaginationRequest"/> object for pagination</param>
    /// <returns>Care Plans are returned in a <see cref="PaginatedResult{T}"/> object</returns>
    Task<PaginatedResult<IEnumerable<Resource>>> GetCarePlans(string patientId, PaginationRequest paginationRequest);

    /// <summary>
    /// Gets a single <see cref="CarePlan"/>, or a null value if the Care Plan does not exist
    /// </summary>
    /// <param name="id">The Care PLan's ID</param>
    /// <returns>A <see cref="CarePlan"/> or otherwise</returns>
    Task<CarePlan?> GetCarePlan(string id);

    /// <summary>
    /// Updates a <see cref="CarePlan"/>. This method does not support partial updates
    /// </summary>
    /// <param name="id">The Care Plan ID</param>
    /// <param name="carePlan">The updated <see cref="CarePlan"/></param>
    /// <returns>A boolean value, true if the update was successful, false otherwise</returns>
    Task<bool> UpdateCarePlan(string id, CarePlan carePlan);

    /// <summary>
    /// Deletes a Care Plan
    /// </summary>
    /// <param name="id">The Care Plan ID to look for</param>
    /// <returns>A boolean value, true if the delete was successful, false otherwise</returns>
    Task<bool> DeleteCarePlan(string id);
}
