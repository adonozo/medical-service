namespace QMUL.DiabetesBackend.Model.FHIR;

using Hl7.Fhir.Model;

public record Coding(string System, string Code, string Display)
{
    public Hl7.Fhir.Model.Coding ToFhirCoding() =>
        new(this.System, this.Code, this.Display);
}

public record Code(Coding Coding)
{
    public CodeableConcept ToFhirCodeableConcept() =>
        new(this.Coding.System, this.Coding.Code, this.Coding.Display, string.Empty);
}