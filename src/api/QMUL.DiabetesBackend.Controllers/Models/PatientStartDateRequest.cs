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
}