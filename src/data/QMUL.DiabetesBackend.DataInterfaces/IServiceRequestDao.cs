namespace QMUL.DiabetesBackend.DataInterfaces;

using System.Collections.Generic;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using Model.Exceptions;
using NodaTime;

/// <summary>
/// The Service Request Dao interface.
/// </summary>
public interface IServiceRequestDao
{
    /// <summary>
    /// Creates a service request. The ID is assigned in the database.
    /// </summary>
    /// <param name="newRequest">The <see cref="ServiceRequest"/> to insert.</param>
    /// <returns>The new <see cref="ServiceRequest"/> with a generated ID.</returns>
    /// <exception cref="WriteResourceException">If the service request could not be created.</exception>
    Task<ServiceRequest> CreateServiceRequest(ServiceRequest newRequest);

    /// <summary>
    /// Gets a single <see cref="ServiceRequest"/> given an ID.
    /// </summary>
    /// <param name="id">The service request ID.</param>
    /// <returns>A single <see cref="ServiceRequest"/></returns>
    Task<ServiceRequest?> GetServiceRequest(string id);

    /// <summary>
    /// Gets the <see cref="ServiceRequest"/> list with a active status for a given patient.
    /// </summary>
    /// <param name="patientId">The patient ID.</param>
    /// <returns>A list of <see cref="ServiceRequest"/>.</returns>
    Task<IList<ServiceRequest>> GetActiveServiceRequests(string patientId);

    /// <summary>
    /// Updates a given <see cref="ServiceRequest"/> 
    /// </summary>
    /// <param name="id">The service request ID.</param>
    /// <param name="actualRequest">The service request with updated data.</param>
    /// <returns>A bool indicating the result.</returns>
    Task<bool> UpdateServiceRequest(string id, ServiceRequest actualRequest);

    /// <summary>
    /// Updates the status of multiple service requests
    /// </summary>
    /// <param name="ids">The IDs of the service requests to update</param>
    /// <param name="status">The new <see cref="RequestStatus"/></param>
    /// <returns>A bool indicating the result.</returns>
    Task<bool> UpdateServiceRequestsStatus(string[] ids, RequestStatus status);

    /// <summary>
    /// Sets the start date for a number of service requests. It will replace the OccurrenceTiming Extensions with a
    /// single extension containing the start date, meaning that `NeedsStartDateFlag` or `NeedsStartTimeFlag` will be
    /// overriden.
    /// </summary>
    /// <param name="ids">The list of service request IDs to update</param>
    /// <param name="date">The local date in ISO format</param>
    /// <returns>A bool indicating the result.</returns>
    Task<bool> UpdateServiceRequestsStartDate(string[] ids, LocalDate date);

    /// <summary>
    /// Deletes a service request from the database.
    /// </summary>
    /// <param name="id">The service request ID to delete.</param>
    /// <returns>A boolean with the result.</returns>
    Task<bool> DeleteServiceRequest(string id);

    /// <summary>
    /// Deletes multiple service requests given an array of IDs
    /// </summary>
    /// <param name="ids">The array of service requests IDs to delete</param>
    /// <returns>A boolean value to indicate if all of the deletes were successful</returns>
    Task<bool> DeleteServiceRequests(string[] ids);

    /// <summary>
    /// Gets a list of service request given an array of IDs.
    /// </summary>
    /// <param name="ids">The array of IDs</param>
    /// <returns>A list of service requests matching the IDs.</returns>
    Task<IList<ServiceRequest>> GetServiceRequestsByIds(string[] ids);
}