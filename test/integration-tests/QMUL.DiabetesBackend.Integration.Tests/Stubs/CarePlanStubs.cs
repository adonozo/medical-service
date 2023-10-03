namespace QMUL.DiabetesBackend.Integration.Tests.Stubs;

using Hl7.Fhir.Model;

public static class CarePlanStubs
{
    public static CarePlan CarePlan(string patientId) => new()
    {
        Subject = new ResourceReference
        {
            Display = "John Doe",
            Reference = $"Patient/{patientId}"
        },
    };
}