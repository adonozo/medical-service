namespace QMUL.DiabetesBackend.Model.FHIR;

using Hl7.Fhir.Model;

public record Reference(ValueQuantity Low, ValueQuantity High)
{
    public Range ToFhirRange() =>
        new(this.Low.ToFhirQuantity(), this.High.ToFhirQuantity());
}