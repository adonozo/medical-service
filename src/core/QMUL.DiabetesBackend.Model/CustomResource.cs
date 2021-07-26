using QMUL.DiabetesBackend.Model.Enums;

namespace QMUL.DiabetesBackend.Model
{
    /// <summary>
    /// The resource it is referred to
    /// </summary>
    public class CustomResource
    {
        /// <summary>
        /// The event type
        /// </summary>
        public EventType EventType { get; set; }
        
        /// <summary>
        /// The main DomainResource's ID. e.g., the medication or service request ID. 
        /// </summary>
        public string ResourceId { get; set; }
        
        /// <summary>
        /// The ID of the related resource, e.g., the dosageInstruction ID from the medication request.
        /// </summary>
        public string EventReferenceId { get; set; }

        /// <summary>
        /// The medication name or the measurement instruction
        /// </summary>
        public string Text { get; set; }
    }
}