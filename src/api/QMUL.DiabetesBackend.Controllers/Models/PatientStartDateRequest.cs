namespace QMUL.DiabetesBackend.Controllers.Models;

using NodaTime;

/// <summary>
/// The model to get the start date from a request body
/// </summary>
public class PatientStartDateRequest
{
    /// <summary>
    /// The request start date
    /// </summary>
    public LocalDate StartDate { get; set; }

    /// <summary>
    /// The request start time, needed when the resource occurs multiple times a day and does not have defined times
    /// </summary>
    public LocalTime? StartTime { get; set; }
}