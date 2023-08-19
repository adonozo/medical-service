namespace QMUL.DiabetesBackend.Service.Tests.Stubs;

using System;
using System.Collections.Generic;
using Hl7.Fhir.Model;

public static class MedicationRequestStubs
{
    public static MedicationRequest ValidMedicationRequestAtFixedTime(string medicationRequestId = null,
        string dosageId = null,
        DataType period = null) => new()
    {
        Id = medicationRequestId ?? Guid.NewGuid().ToString(),
        DosageInstruction = new List<Dosage>
        {
            new()
            {
                ElementId = dosageId ?? Guid.NewGuid().ToString(),
                Timing = new Timing
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
                }
            }
        }
    };
}