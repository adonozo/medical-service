namespace QMUL.DiabetesBackend.Service.Tests.Stubs;

using System;
using System.Collections.Generic;
using Hl7.Fhir.Model;

public static class ServiceRequestStubs
{
    public static ServiceRequest ValidPeriodAtFixedTime(Period period = null)
    {
        var timing = new Timing
        {
            Repeat = new Timing.RepeatComponent
            {
                PeriodUnit = Timing.UnitsOfTime.D,
                Period = 1,
                Frequency = 1,
                Bounds = period ?? new Period
                {
                    Start = "2023-01-01",
                    End = "2023-01-10"
                },
                TimeOfDay = new[] { "10:00" }
            }
        };
        var contained = new ServiceRequest
        {
            Id = Guid.NewGuid().ToString(),
            Occurrence = timing
        };

        return new ServiceRequest
        {
            Id = Guid.NewGuid().ToString(),
            Contained = new List<Resource> { contained },
            Occurrence = timing
        };
    }
}