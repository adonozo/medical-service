namespace QMUL.DiabetesBackend.Service.Tests;

using System;
using System.Collections.Generic;
using Hl7.Fhir.Model;
using Model;
using Model.Enums;
using NodaTime;
using Duration = Hl7.Fhir.Model.Duration;
using Period = Hl7.Fhir.Model.Period;

public static class TestUtils
{
    public static Patient GetStubPatient()
    {
        return new Patient
        {
            Id = Guid.NewGuid().ToString(),
            Name = new List<HumanName> { HumanName.ForFamily("Doe") }
        };
    }

    public static InternalPatient GetStubInternalPatient()
    {
        return new InternalPatient
        {
            Id = Guid.NewGuid().ToString(),
            ExactEventTimes = new Dictionary<CustomEventTiming, LocalTime>()
        };
    }

    public static Timing ValidFrequencyDurationTiming(int frequency = 1, int days = 10) => new()
    {
        Repeat = new Timing.RepeatComponent
        {
            PeriodUnit = Timing.UnitsOfTime.D,
            Period = 1,
            Frequency = frequency,
            Bounds = new Duration
            {
                Unit = "d",
                Value = days
            }
        }
    };

    public static Timing ValidPeriodDurationTiming(Period period) => new()
    {
        Repeat = new Timing.RepeatComponent
        {
            PeriodUnit = Timing.UnitsOfTime.D,
            Period = 1,
            Frequency = 1,
            Bounds = period
        }
    };
}