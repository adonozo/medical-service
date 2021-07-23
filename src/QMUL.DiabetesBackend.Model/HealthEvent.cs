using System;

namespace QMUL.DiabetesBackend.Model
{
    /// <summary>
    /// Holds an individual patient related event, i.e., a dosage in a medication request or a service request. 
    /// </summary>
    public class HealthEvent
    {
        public string Id { get; set; }
        
        public DateTime EventDateTime { get; set; }
        
        public CustomResource Resource { get; set; }
    }
}