using System;
using System.Collections.Generic;
using QMUL.DiabetesBackend.Model.Enums;

namespace QMUL.DiabetesBackend.Model
{
    public class Patient
    {
        public string Id { get; set; }
        
        public string AlexaUserId { get; set; }
        
        public string FirstName { get; set; }
        
        public string LastName { get; set; }
        
        public string Email { get; set; }
        
        /// <summary>
        /// Holds the exact time the patient has set a specific event in the day: i.e., breakfast, diner, sleep. The date
        /// in the datetime is ignored. 
        /// </summary>
        public Dictionary<CustomEventTiming, DateTime> ExactEventTimes { get; set; }

        /// <summary>
        /// Holds the exact date for a resource to start. Should be used when the resource has a frequency rather than a
        /// period. For example, a medication that must be taken for 14 days. The key is the related resource ID, i.e.,
        /// the dosage ID for a medication request, or the service request ID for a measurement.  
        /// </summary>
        public Dictionary<string, DateTime> ResourceStartDate { get; set; }
    }
}