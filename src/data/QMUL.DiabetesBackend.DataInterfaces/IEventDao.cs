using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using QMUL.DiabetesBackend.Model;
using QMUL.DiabetesBackend.Model.Enums;

namespace QMUL.DiabetesBackend.DataInterfaces
{
    public interface IEventDao
    {
        public Task<bool> CreateEvents(IEnumerable<HealthEvent> events);

        /// <summary>
        /// Deletes the event series based on a reference ID.
        /// </summary>
        /// <param name="referenceId">The Reference ID. i.e., the dosageInstruction ID from the medication request.</param>
        /// <returns>If the delete operation was successful.</returns>
        public Task<bool> DeleteEventSeries(string referenceId);

        /// <summary>
        /// Updates all events' times that matches the timing. Only newer events. 
        /// </summary>
        /// <param name="patientId">The patient ID</param>
        /// <param name="timing">The timing to match</param>
        /// <param name="time">The time to change. Date is ignored</param>
        /// <returns>A boolean value to indicate if the update was successful.</returns>
        public Task<bool> UpdateEventsTiming(string patientId, CustomEventTiming timing, DateTime time);

        public Task<IEnumerable<HealthEvent>> GetEvents(string patientId, EventType type, DateTime start, DateTime end);

        public Task<IEnumerable<HealthEvent>> GetEvents(string patientId, EventType type, DateTime start, DateTime end,
            CustomEventTiming[] timings);

        public Task<IEnumerable<HealthEvent>> GetEvents(string patientId, EventType[] types, DateTime start, DateTime end);

        public Task<IEnumerable<HealthEvent>> GetEvents(string patientId, EventType[] types, DateTime start, DateTime end,
            CustomEventTiming[] timings);

        public Task<IEnumerable<HealthEvent>> GetNextEvents(string patientId, EventType type);

        public Task<IEnumerable<HealthEvent>> GetNextEvents(string patientId, EventType[] types);
    }
}