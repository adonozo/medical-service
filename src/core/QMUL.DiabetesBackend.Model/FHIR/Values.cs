namespace QMUL.DiabetesBackend.Model.FHIR;

public record ValueQuantity(string Unit, string System, string Code)
{
    public Hl7.Fhir.Model.Quantity ToFhirQuantity() =>
        new (value: 0, this.Unit, this.System);
}
