namespace QMUL.DiabetesBackend.Service.Tests;

using System;
using System.Collections.Generic;
using Hl7.Fhir.Model;
using Model;
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
            Id = Guid.NewGuid().ToString()
        };
    }

    public static Timing ValidPeriodDurationTiming(Period period) => new()
    {
        Repeat = new Timing.RepeatComponent
        {
            PeriodUnit = Timing.UnitsOfTime.D,
            Period = 1,
            Frequency = 1,
            Bounds = period,
            TimeOfDay = new []{ "10:00" }
        }
    };
}