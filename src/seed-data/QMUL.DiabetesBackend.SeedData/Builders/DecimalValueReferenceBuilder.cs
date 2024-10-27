namespace QMUL.DiabetesBackend.SeedData.Builders;

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
                Unit: this.ValueUnit,
                System: CustomCodes.UnitsSystem,
                Code: this.ValueCode,
                Value: this.LowValue),
            High: new DecimalValueQuantity(
                Unit: this.ValueUnit,
                System: CustomCodes.UnitsSystem,
                Code: this.ValueCode,
                Value: this.HighValue),
            AppliesTo: this.AppliesTo);
    }
}