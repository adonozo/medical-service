using System;
using QMUL.DiabetesBackend.Model.Enums;

namespace QMUL.DiabetesBackend.Model
{
    /// <summary>
    /// Holds an individual patient related event, i.e., a dosage in a medication request or a service request. 
    /// </summary>
    public class HealthEvent
    {
        public string Id { get; set; }
        
        public string PatientId { get; set; }
        
        public DateTime EventDateTime { get; set; }
        
        /// <summary>
        /// True if the events happens at an exact time, or if the patient has declared exact times for ambiguous times
        /// such as breakfast, lunch, etc.
        /// False if the patient hasn't declared an exact time for such events.
        /// </summary>
        public bool ExactTimeIsSetup { get; set; }
        
        /// <summary>
        /// When this occurs
        /// </summary>
        public CustomEventTiming EventTiming { get; set; }
        
        public CustomResource Resource { get; set; }
    }
}