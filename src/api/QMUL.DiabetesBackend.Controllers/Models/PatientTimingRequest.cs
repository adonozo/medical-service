namespace QMUL.DiabetesBackend.Controllers.Models;

using Model.Enums;
using NodaTime;

/// <summary>
/// The patient's request to update a timing value, e.g., breakfast 
/// </summary>
public class PatientTimingRequest
{
    /// <summary>
    /// The custom event timing to update
    /// </summary>
    public CustomEventTiming Timing { get; set; }

    /// <summary>
    /// The new time for the timing event
    /// </summary>
    public LocalTime LocalTime { get; set; }
}