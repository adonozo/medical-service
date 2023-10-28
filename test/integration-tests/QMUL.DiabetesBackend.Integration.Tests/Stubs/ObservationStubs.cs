namespace QMUL.DiabetesBackend.Integration.Tests.Stubs;

using System;
using System.Collections.Generic;
using Hl7.Fhir.Model;

public static class ObservationStubs
{
    public static Observation BloodGlucoseReading(string patientId) => new()
    {
        Status = ObservationStatus.Final,
        Code = new CodeableConcept
        {
            Coding = new List<Coding>
            {
                new()
                {
                    Display = "Glucose [Moles/volume] in Blood",
                    System = "http://loinc.org",
                    Code = "15074-8"
                }
            }
        },
        Subject = new ResourceReference
        {
            Display = "John Doe",
            Reference = $"Patient/{patientId}"
        },
        Issued = new DateTimeOffset(2020, 06, 01, 10, 00, 00, TimeSpan.Zero),
        Effective = new FhirDateTime(2020, 06, 01, 10, 00, 00, TimeSpan.Zero),
        Value = new Quantity
        {
            Code = "mmol/L",
            System = "http://unitsofmeasure.org",
            Unit = "mmol/l",
            Value = 4.5m
        },
        ReferenceRange = new List<Observation.ReferenceRangeComponent>
        {
            new ()
            {
                Low = new Quantity
                {
                    Code = "mmol/L",
                    System = "http://unitsofmeasure.org",
                    Unit = "mmol/l",
                    Value = 3.1m
                },
                High = new Quantity
                {
                    Code = "mmol/L",
                    System = "http://unitsofmeasure.org",
                    Unit = "mmol/l",
                    Value = 6.2m
                }
            }
        }
    };
}