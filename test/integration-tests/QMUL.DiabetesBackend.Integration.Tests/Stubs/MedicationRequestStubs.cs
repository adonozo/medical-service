namespace QMUL.DiabetesBackend.Integration.Tests.Stubs;

using System.Collections.Generic;
using Hl7.Fhir.Model;

public static class MedicationRequestStubs
{
    public static MedicationRequest MetforminRequest(string patientId, string medicationId) => new()
    {
        Priority = RequestPriority.Routine,
        Status = MedicationRequest.MedicationrequestStatus.Active,
        Intent = MedicationRequest.MedicationRequestIntent.Order,
        AuthoredOn = "2020-01-01",
        Subject = new ResourceReference
        {
            Display = "John Doe",
            Reference = $"Patient/{patientId}"
        },
        Medication = new CodeableReference
        {
            Reference = new ResourceReference
            {
                Reference = $"Medication/{medicationId}"
            }
        },
        DosageInstruction = new List<Dosage>
        {
            new()
            {
                Timing = TimingStubs.FourWeeksDaily,
                DoseAndRate = new List<Dosage.DoseAndRateComponent>
                {
                    new()
                    {
                        Type = new()
                        {
                            Coding = new List<Coding>
                            {
                                new()
                                {
                                    System = "http://terminology.hl7.org/CodeSystem/dose-rate-type",
                                    Code = "ordered",
                                    Display = "Ordered"
                                }
                            }
                        },
                        Dose = new Quantity
                        {
                            Value = 1,
                            Unit = "TAB",
                            Code = "TAB",
                            System = "http://terminology.hl7.org/CodeSystem/v3-orderableDrugForm"
                        }
                    }
                }
            }
        }
    };
}