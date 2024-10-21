namespace QMUL.DiabetesBackend.SeedData;

using Model.FHIR;

public class DecimalValueReferenceBuilder : ReferenceBuilderBase
{
    public DecimalValueReferenceBuilder(string valueUnit, string valueCode) : base(valueUnit, valueCode)
    {
    }

    public override Reference Build()
    {
        return new Reference(
            Low: new DecimalValueQuantity(
                Unit: this.valueUnit,
                System: CustomCodes.UnitsSystem,
                Code: this.valueCode,
                Value: this.low),
            High: new DecimalValueQuantity(
                Unit: this.valueUnit,
                System: CustomCodes.UnitsSystem,
                Code: this.valueCode,
                Value: this.high),
            AppliesTo: this.AppliesTo);
    }
}