namespace QMUL.DiabetesBackend.DataInterfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Exceptions;
    using Hl7.Fhir.Model;

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
        /// <exception cref="CreateException">If the service request could not be created.</exception>
        public Task<ServiceRequest> CreateServiceRequest(ServiceRequest newRequest);

        /// <summary>
        /// Gets a single <see cref="ServiceRequest"/> given an ID.
        /// </summary>
        /// <param name="id">The service request ID.</param>
        /// <returns>A single <see cref="ServiceRequest"/></returns>
        /// <exception cref="NotFoundException">If the service request was not found.</exception>
        public Task<ServiceRequest> GetServiceRequest(string id);

        /// <summary>
        /// Gets the list of all <see cref="ServiceRequest"/> for a given patient.
        /// </summary>
        /// <param name="patientId">The patient ID</param>
        /// <returns>A list of <see cref="ServiceRequest"/>.</returns>
        public Task<List<ServiceRequest>> GetServiceRequestsFor(string patientId);

        /// <summary>
        /// Gets the <see cref="ServiceRequest"/> list with a active status for a given patient.
        /// </summary>
        /// <param name="patientId">The patient ID.</param>
        /// <returns>A list of <see cref="ServiceRequest"/>.</returns>
        public Task<List<ServiceRequest>> GetActiveServiceRequests(string patientId);

        /// <summary>
        /// Updates a given <see cref="ServiceRequest"/> 
        /// </summary>
        /// <param name="id">The service request ID.</param>
        /// <param name="actualRequest">The service request with updated data.</param>
        /// <returns>The updated <see cref="ServiceRequest"/>.</returns>
        /// <exception cref="UpdateException">If the service request was not updated.</exception>
        public Task<ServiceRequest> UpdateServiceRequest(string id, ServiceRequest actualRequest);

        /// <summary>
        /// Deletes a service request from the database.
        /// </summary>
        /// <param name="id">The service request ID to delete.</param>
        /// <returns>A boolean with the result.</returns>
        public Task<bool> DeleteServiceRequest(string id);

        /// <summary>
        /// Gets a list of service request given an array of IDs.
        /// </summary>
        /// <param name="ids">The array of IDs</param>
        /// <returns>A list of service requests matching the IDs.</returns>
        public Task<List<ServiceRequest>> GetServiceRequestsByIds(string[] ids);
    }
}