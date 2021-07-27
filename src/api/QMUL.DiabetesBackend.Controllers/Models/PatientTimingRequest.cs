using System;
using QMUL.DiabetesBackend.Model.Enums;

namespace QMUL.DiabetesBackend.Api.Models
{
    public class PatientTimingRequest
    {
        public CustomEventTiming Timing { get; set; }
        
        public DateTime DateTime { get; set; }
    }
}