namespace QMUL.DiabetesBackend.Model;

using Enums;
using NodaTime;

/// <summary>
/// A custom resource that references an actual FHIR resource
/// </summary>
public class ResourceReference
{
    /// <summary>
    /// The event type
    /// </summary>
    public EventType EventType { get; set; }

    /// <summary>
    /// The main DomainResource's ID. e.g., the medication or service request ID. 
    /// </summary>
    public string DomainResourceId { get; set; }

    /// <summary>
    /// The ID of the related resource, e.g., the dosageInstruction ID from the medication request.
    /// </summary>
    public string EventReferenceId { get; set; }

    /// <summary>
    /// The medication or measurement instruction
    /// </summary>
    public string Text { get; set; }

    /// <summary>
    /// When this resource has started. Null if it has not started yet.
    /// </summary>
    public LocalDate? StartDate { get; set; } // TODO might not be necessary anymore
}