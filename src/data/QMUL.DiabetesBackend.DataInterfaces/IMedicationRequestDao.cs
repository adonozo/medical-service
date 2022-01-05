namespace QMUL.DiabetesBackend.DataInterfaces
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Hl7.Fhir.Model;

    /// <summary>
    /// The Medication Request Dao interface.
    /// </summary>
    public interface IMedicationRequestDao
    {
        /// <summary>
        /// Creates the medication request from the argument.
        /// </summary>
        /// <param name="newRequest">The <see cref="MedicationRequest"/> to create.</param>
        /// <returns>The newly created medication request.</returns>
        public Task<MedicationRequest> CreateMedicationRequest(MedicationRequest newRequest);

        /// <summary>
        /// Updates a medication request.
        /// </summary>
        /// <param name="id">The medication request's ID</param>
        /// <param name="actualRequest">The <see cref="MedicationRequest"/> to update</param>
        /// <returns>The updated medication request</returns>
        public Task<MedicationRequest> UpdateMedicationRequest(string id, MedicationRequest actualRequest);

        /// <summary>
        /// Gets a <see cref="MedicationRequest"/> based on the ID.
        /// </summary>
        /// <param name="id">The medication request's ID.</param>
        /// <returns>The medication request found.</returns>
        /// <exception cref="KeyNotFoundException">If the medication request was not found.</exception>
        public Task<MedicationRequest> GetMedicationRequest(string id);

        public Task<List<MedicationRequest>> GetMedicationRequestsByIds(string[] ids);

        public Task<List<MedicationRequest>> GetMedicationRequestFor(string patientId);

        public Task<bool> DeleteMedicationRequest(string id);
        
        public Task<MedicationRequest> GetMedicationRequestForDosage(string patientId, string dosageId);

        /// <summary>
        /// Gets the active medication requests for the patient, i.e., the ones that the patient needs to follow. Does
        /// not include insulin request
        /// </summary>
        /// <param name="patientId">The patient's user ID, not email</param>
        /// <returns>The list of active medication requests.</returns>
        public Task<List<MedicationRequest>> GetActiveMedicationRequests(string patientId);
        
        /// <summary>
        /// Gets the all the active medication requests, insulin and non-insulin.
        /// </summary>
        /// <param name="patientId">The patient's user ID, not email</param>
        /// <returns>The list of active medication requests.</returns>
        public Task<List<MedicationRequest>> GetAllActiveMedicationRequests(string patientId);
    }
}