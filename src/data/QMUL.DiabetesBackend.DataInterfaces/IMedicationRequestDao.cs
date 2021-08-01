using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hl7.Fhir.Model;

namespace QMUL.DiabetesBackend.DataInterfaces
{
    public interface IMedicationRequestDao
    {
        public Task<MedicationRequest> CreateMedicationRequest(MedicationRequest newRequest);

        public Task<MedicationRequest> UpdateMedicationRequest(string id, MedicationRequest actualRequest);

        public Task<MedicationRequest> GetMedicationRequest(string id);

        public Task<List<MedicationRequest>> GetMedicationRequestsByIds(string[] ids);

        /// <summary>
        /// Get the list of medications the patient needs to take at a specific time, using an offset (in minutes) to
        /// look for.
        /// </summary>
        /// <param name="patientId">The patient's user ID, not email</param>
        /// <param name="dateTime">The date and time to look for</param>
        /// <param name="intervalMin">Offset minutes to look for. E.g., if dateTime is 12:30, will look between 12:20 and 12:40</param>
        /// <returns>The patient's medication list for that period of time</returns>
        public Task<List<MedicationRequest>> GetMedicationRequestFor(string patientId, DateTime dateTime, int intervalMin);

        /// <summary>
        /// Get the list of medications the patient needs to take at a timing event for a certain date
        /// </summary>
        /// <param name="patientId">The patient's user ID, not email</param>
        /// <param name="dateTime">The date to look for. Time is ignored</param>
        /// <param name="timing">A timing event, e.g., Morning</param>
        /// <returns>The patient's medication list for that period of time</returns>
        public Task<List<MedicationRequest>> GetMedicationRequestFor(string patientId, DateTime dateTime,
            Timing.EventTiming timing);
        
        /// <summary>
        /// Get the list of medications the patient needs to take at a range of dates. Should be used for all-day queries
        /// </summary>
        /// <param name="patientId">The patient's user ID, not email</param>
        /// <param name="startTime">The start time interval</param>
        /// <returns>The patient's medication list for that period of time</returns>
        public Task<List<MedicationRequest>> GetMedicationRequestFor(string patientId, DateTime startTime);
        
        /// <summary>
        /// Gets the next set of medications for the patient to take. Close time medications should be gathered, e.g., 
        /// before and after breakfast.
        /// </summary>
        /// <param name="patientId">The patient's user ID, not email</param>
        /// <returns>The patient's medication list to take next</returns>
        public Task<List<MedicationRequest>> GetNextMedicationRequestFor(string patientId);

        public Task<bool> DeleteMedicationRequest(string id);
        
        public Task<MedicationRequest> GetMedicationRequestForDosage(string patientId, string dosageId);

        /// <summary>
        /// Gets the active medication requests for the patient, i.e., the ones that the patient needs to follow. Does
        /// not include insulin request
        /// </summary>
        /// <param name="patientId">The patient's user ID, not email</param>
        /// <returns>The list of active medication requests.</returns>
        public Task<List<MedicationRequest>> GetActiveMedicationRequests(string patientId);
    }
}