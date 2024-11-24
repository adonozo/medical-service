namespace QMUL.DiabetesBackend.SeedData.Builders;

using Model.FHIR;

public abstract class ReferenceBuilderBase
{
    protected readonly string ValueUnit;
    protected readonly string ValueCode;
    protected readonly List<Code> AppliesTo;

    protected decimal HighValue;
    protected decimal LowValue;

    public ReferenceBuilderBase(string valueUnit, string valueCode)
    {
        this.ValueUnit = valueUnit;
        this.ValueCode = valueCode;
        AppliesTo = new List<Code>();
    }

    public ReferenceBuilderBase High(decimal value)
    {
        this.HighValue = value;
        return this;
    }

    public ReferenceBuilderBase Low(decimal value)
    {
        this.LowValue = value;
        return this;
    }

    public ReferenceBuilderBase AppliesToMen()
    {
        AppliesTo.Add(new Code(new Coding(
            System: CustomCodes.GenderSystem,
            Code: "male",
            Display: "Varon")));

        return this;
    }

    public ReferenceBuilderBase AppliesToWomen()
    {
        AppliesTo.Add(new Code(new Coding(
            System: CustomCodes.GenderSystem,
            Code: "male",
            Display: "Mujer")));

        return this;
    }

    public abstract Reference Build();
}