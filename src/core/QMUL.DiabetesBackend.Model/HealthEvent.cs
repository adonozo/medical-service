namespace QMUL.DiabetesBackend.Model;

using Enums;
using NodaTime;

/// <summary>
/// Holds an individual patient related event, i.e., a dosage in a medication request or a service request. 
/// </summary>
public class HealthEvent
{
    /// <summary>
    /// When this event has to occur.
    /// </summary>
    public LocalDateTime ScheduledDateTime { get; set; }

    /// <summary>
    /// The timing when this occurs. E.g., ACM, CD, etc.
    /// </summary>
    public CustomEventTiming EventTiming { get; set; }
}