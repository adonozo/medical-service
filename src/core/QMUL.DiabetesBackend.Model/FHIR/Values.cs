namespace QMUL.DiabetesBackend.Model.FHIR;

public record ValueQuantity(string Unit, string System, string Code)
{
    public Hl7.Fhir.Model.Quantity ToFhirQuantity() =>
        new (value: 0, this.Unit, this.System);
}

public record DecimalValueQuantity(string Unit, string System, string Code, decimal Value)
    : ValueQuantity(Unit, System, Code)
{
    public Hl7.Fhir.Model.Quantity ToFhirQuantity() =>
        new (Value, this.Unit, this.System);
}