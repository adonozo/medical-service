using System;
using System.Collections.Generic;
using Hl7.Fhir.Model;

namespace QMUL.DiabetesBackend.DataInterfaces
{
    public interface IMedicationRequestDao
    {
        public MedicationRequest CreateMedicationRequest(MedicationRequest newRequest);

        public MedicationRequest UpdateMedicationRequest(string id, MedicationRequest actualRequest);

        public MedicationRequest GetMedicationRequest(string id);

        /// <summary>
        /// Get the list of medications the patient needs to take at a specific time, using an offset (in minutes) to
        /// look for.
        /// </summary>
        /// <param name="patientId">The patient's user ID, not email</param>
        /// <param name="dateTime">The date and time to look for</param>
        /// <param name="intervalMin">Offset minutes to look for. E.g., if dateTime is 12:30, will look between 12:20 and 12:40</param>
        /// <returns>The patient's medication list for that period of time</returns>
        public List<MedicationRequest> GetMedicationRequestFor(string patientId, DateTime dateTime, int intervalMin = 10);

        /// <summary>
        /// Get the list of medications the patient needs to take at a timing event for a certain date
        /// </summary>
        /// <param name="patientId">The patient's user ID, not email</param>
        /// <param name="dateTime">The date to look for. Time is ignored</param>
        /// <param name="timing">A timing event, e.g., Morning</param>
        /// <returns>The patient's medication list for that period of time</returns>
        public List<MedicationRequest> GetMedicationRequestFor(string patientId, DateTime dateTime,
            Timing.EventTiming timing);
        
        /// <summary>
        /// Get the list of medications the patient needs to take at a range of dates
        /// </summary>
        /// <param name="patientId">The patient's user ID, not email</param>
        /// <param name="startTime">The start time interval</param>
        /// <param name="endTime">The end time interval</param>
        /// <returns>The patient's medication list for that period of time</returns>
        public List<MedicationRequest> GetMedicationRequestFor(string patientId, DateTime startTime, DateTime endTime);
        
        /// <summary>
        /// Gets the next set of medications for the patient to take. Close time medications should be gathered, e.g., 
        /// before and after breakfast.
        /// </summary>
        /// <param name="patientId">The patient's user ID, not email</param>
        /// <returns>The patient's medication list to take next</returns>
        public List<MedicationRequest> GetNextMedicationRequestFor(string patientId);

        public bool DeleteMedicationRequest(string id);
    }
}