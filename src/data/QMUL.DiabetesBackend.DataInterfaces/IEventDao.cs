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
        /// Updates a single event with a given ID.
        /// </summary>
        /// <param name="eventId">The event's ID</param>
        /// <param name="healthEvent">The event to update</param>
        /// <returns>If the update was successful</returns>
        public Task<bool> UpdateEvent(string eventId, HealthEvent healthEvent);

        /// <summary>
        /// Deletes the event series based on a reference ID.
        /// </summary>
        /// <param name="referenceId">The Reference ID. i.e., the dosageInstruction ID from the medication request.</param>
        /// <returns>If the delete operation was successful.</returns>
        public Task<bool> DeleteEventSeries(string referenceId);

        /// <summary>
        /// Gets the series of events given a reference ID. For example, a dosage instruction will have its series of events
        /// </summary>
        /// <param name="referenceId">The reference ID. i.e., the dosageInstruction ID from the medication request.</param>
        /// <returns>The list of events</returns>
        public Task<IEnumerable<HealthEvent>> GetEvents(string referenceId);

        /// <summary>
        /// Gets the events for a patient given a date.
        /// </summary>
        /// <param name="patientId">The patient ID</param>
        /// <param name="dateTime">The date when the events happens</param>
        /// <param name="offset">An offset time in minutes to look for</param>
        /// <returns>The list of events for that datetime</returns>
        public Task<IEnumerable<HealthEvent>> GetEvents(string patientId, DateTime dateTime, int offset);
        
        /// <summary>
        /// Get events for the given event type: Medication, Insulin, Measure; for a given date.
        /// </summary>
        /// <returns>The list of events for a given date.</returns>
        public Task<IEnumerable<HealthEvent>> GetEvents(string patientId, EventType type, DateTime dateTime, int offset);
        
        /// <summary>
        /// Gets the events for a patient given a time in the day. E.g., before breakfast, at night, etc.
        /// </summary>
        /// <param name="patientId">The patient ID</param>
        /// <param name="dateTime">The date when the events happens. Time is ignored</param>
        /// <param name="time">When the event happens: before breakfast, at night, etc.</param>
        /// <returns>The list of events for that date and event.</returns>
        public Task<IEnumerable<HealthEvent>> GetEvents(string patientId, DateTime dateTime, CustomEventTiming time);
        
        /// <summary>
        /// Gets the events for the given type: Medication, Insulin, Measure; for a time in the day.
        /// </summary>
        /// <returns>The lists of events for that date and event</returns>
        public Task<IEnumerable<HealthEvent>> GetEvents(string patientId, EventType type, DateTime dateTime, CustomEventTiming time);
        
        /// <summary>
        /// Gets all events for a given day.
        /// </summary>
        /// <param name="patientId">The patient ID</param>
        /// <param name="dateTime">The date to look for, without considering the time.</param>
        /// <returns>The list of events for that day.</returns>
        public Task<IEnumerable<HealthEvent>> GetEvents(string patientId, DateTime dateTime);

        /// <summary>
        /// Gets all events for a given day, all day; considering an event type: Medication, Insulin, Measure.
        /// </summary>
        /// <returns>The list of events for that day.</returns>
        public Task<IEnumerable<HealthEvent>> GetEvents(string patientId, EventType type, DateTime dateTime);

        /// <summary>
        /// Updates all events' times that matches the timing. Only newer events. 
        /// </summary>
        /// <param name="patientId">The patient ID</param>
        /// <param name="timing">The timing to match</param>
        /// <param name="time">The time to change. Date is ignored</param>
        /// <returns>A boolean value to indicate if the update was successful.</returns>
        public Task<bool> UpdateEventsTiming(string patientId, CustomEventTiming timing, DateTime time);
    }
}