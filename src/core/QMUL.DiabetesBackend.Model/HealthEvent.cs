namespace QMUL.DiabetesBackend.Model;

using Enums;
using Hl7.Fhir.Model;
using NodaTime;

/// <summary>
/// Holds an individual patient related event, i.e., a dosage in a medication request or a service request. 
/// </summary>
public class HealthEvent<T> where T : Resource // TODO create concrete medication/service request classes
{
    /// <summary>
    /// When this event has to occur.
    /// </summary>
    public LocalDateTime ScheduledDateTime { get; set; }

    /// <summary>
    /// The timing when this occurs. E.g., ACM, CD, etc.
    /// </summary>
    public CustomEventTiming EventTiming { get; set; }

    public T Resource { get; init; }

    public Dosage Dosage { get; set; }
}