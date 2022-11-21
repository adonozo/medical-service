namespace QMUL.DiabetesBackend.Controllers.Models;

using System;
using Model.Enums;

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
    /// The datetime for the timing event
    /// </summary>
    public DateTime DateTime { get; set; }
}