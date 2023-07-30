namespace QMUL.DiabetesBackend.Model;

using Enums;
using NodaTime;

/// <summary>
/// Holds an individual patient related event, i.e., a dosage in a medication request or a service request. 
/// </summary>
public class HealthEvent
{
    public string PatientId { get; set; }

    /// <summary>
    /// When this event has to occur.
    /// </summary>
    public LocalDateTime ScheduledDateTime { get; set; }

    /// <summary>
    /// True if the events happens at an exact time, or if the patient has declared exact times for ambiguous times
    /// such as breakfast, lunch, etc.
    /// False if the patient hasn't declared an exact time for such events.
    /// </summary>
    public bool ExactTimeIsSetup { get; set; } // TODO might not be necessary anymore

    /// <summary>
    /// The timing when this occurs. E.g., ACM, CD, etc.
    /// </summary>
    public CustomEventTiming EventTiming { get; set; }

    public ResourceReference ResourceReference { get; set; }
}