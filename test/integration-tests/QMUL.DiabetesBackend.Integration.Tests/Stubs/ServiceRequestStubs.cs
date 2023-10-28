namespace QMUL.DiabetesBackend.Integration.Tests.Stubs;

using System.Collections.Generic;
using Hl7.Fhir.Model;

public static class ServiceRequestStubs
{
    public static ServiceRequest GlucoseMeasureRequest(string patientId)
    {
        var resource = BaseRequest(patientId);
        resource.Occurrence = TimingStubs.FourWeeksNoTime;
        var contained = BaseRequest(patientId);
        contained.Occurrence = TimingStubs.FourWeeksMonTue;
        resource.Contained = new List<Resource> { contained };

        return resource;
    }

    private static ServiceRequest BaseRequest(string patientId) => new()
    {
        Status = RequestStatus.Active,
        Intent = RequestIntent.Plan,
        Subject = new ResourceReference
        {
            Display = "John Doe",
            Reference = $"Patient/{patientId}"
        },
        Code = new CodeableReference
        {
            Concept = new CodeableConcept
            {
                Coding = new List<Coding>
                {
                    new()
                    {
                        System = "http://snomed.info/sct",
                        Code = "36048009",
                        Display = "Glucose measurement"
                    }
                }
            }
        },
    };
}