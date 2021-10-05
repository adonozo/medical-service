namespace QMUL.DiabetesBackend.Api.Models
{
    using System;

    /// <summary>
    /// The model to get the start date from a request body
    /// </summary>
    public class PatientStartDateRequest
    {
        /// <summary>
        /// The request start date
        /// </summary>
        public DateTime StartDate { get; set; }
    }
}