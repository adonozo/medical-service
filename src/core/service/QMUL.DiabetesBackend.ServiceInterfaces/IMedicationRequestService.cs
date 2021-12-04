namespace QMUL.DiabetesBackend.ServiceInterfaces
{
    using System.Threading.Tasks;
    using Hl7.Fhir.Model;

    /// <summary>
    /// The Medication Request Service Interface.
    /// </summary>
    public interface IMedicationRequestService
    {
        /// <summary>
        /// Creates a medication request service.
        /// </summary>
        /// <param name="request">The <see cref="MedicationRequest"/> to create.</param>
        /// <returns>The <see cref="MedicationRequest"/> with an updated ID.</returns>
        public Task<MedicationRequest> CreateMedicationRequest(MedicationRequest request);

        /// <summary>
        /// Gets a single medication request given an ID.
        /// </summary>
        /// <param name="id">The <see cref="MedicationRequest"/> ID to look for.</param>
        /// <returns>The <see cref="MedicationRequest"/> if found. An error otherwise.</returns>
        public Task<MedicationRequest> GetMedicationRequest(string id);

        /// <summary>
        /// Updates a medication request. The ID cannot be updated.
        /// </summary>
        /// <param name="id">The medication request's ID to be updated.</param>
        /// <param name="request">The actual <see cref="MedicationRequest"/> to save.</param>
        /// <returns>The updated <see cref="MedicationRequest"/> if found and updated. An error otherwise.</returns>
        public Task<MedicationRequest> UpdateMedicationRequest(string id, MedicationRequest request);

        /// <summary>
        /// Deletes a medication Request given an ID.
        /// </summary>
        /// <param name="id">The medication request's ID to delete.</param>
        /// <returns>A boolean, true if the operation was successful.</returns>
        public Task<bool> DeleteMedicationRequest(string id);

        /// <summary>
        /// Gets the active medication requests for a given patient.
        /// </summary>
        /// <param name="patientIdOrEmail">The patient's ID or email.</param>
        /// <returns>A <see cref="Bundle"/> object with the list of active medication requests.</returns>
        public Task<Bundle> GetActiveMedicationRequests(string patientIdOrEmail);
    }
}