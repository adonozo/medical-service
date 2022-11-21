namespace QMUL.DiabetesBackend.Service.Tests;

using System;
using System.Collections.Generic;
using Hl7.Fhir.Model;
using Model;
using Model.Enums;

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
            ExactEventTimes = new Dictionary<CustomEventTiming, DateTimeOffset>()
        };
    }
}