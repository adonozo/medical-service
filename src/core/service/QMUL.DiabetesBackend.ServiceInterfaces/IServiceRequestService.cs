namespace QMUL.DiabetesBackend.ServiceInterfaces
{
    using System.Threading.Tasks;
    using Hl7.Fhir.Model;

    /// <summary>
    /// The Service Request Service Interface
    /// </summary>
    public interface IServiceRequestService
    {
        /// <summary>
        /// Creates a <see cref="ServiceRequest"/>.
        /// </summary>
        /// <param name="request">The new <see cref="ServiceRequest"/> to create.</param>
        /// <returns>A <see cref="ServiceRequest"/> object with a new ID.</returns>
        public Task<ServiceRequest> CreateServiceRequest(ServiceRequest request);

        /// <summary>
        /// Gets a single <see cref="ServiceRequest"/> given an ID.
        /// </summary>
        /// <param name="id">The service request's ID to look for.</param>
        /// <returns>A <see cref="ServiceRequest"/> object if found. An error otherwise.</returns>
        public Task<ServiceRequest> GetServiceRequest(string id);

        /// <summary>
        /// Updates a <see cref="ServiceRequest"/>.
        /// </summary>
        /// <param name="id">The service request's ID to look for.</param>
        /// <param name="request">The <see cref="ServiceRequest"/> with updated data.</param>
        /// <returns>The updated <see cref="ServiceRequest"/> if found and updated. An error otherwise.</returns>
        public Task<ServiceRequest> UpdateServiceRequest(string id, ServiceRequest request);

        /// <summary>
        /// Deletes a <see cref="ServiceRequest"/> given an ID.
        /// </summary>
        /// <param name="id">The service request's ID to look for.</param>
        /// <returns>A boolean value to indicate if the request was successful.</returns>
        public Task<bool> DeleteServiceRequest(string id);
    }
}