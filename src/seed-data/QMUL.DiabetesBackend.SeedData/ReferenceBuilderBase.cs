namespace QMUL.DiabetesBackend.SeedData;

using Model.FHIR;

public abstract class ReferenceBuilderBase
{
    protected readonly string valueUnit;
    protected readonly string valueCode;
    protected readonly List<Code> AppliesTo;

    protected decimal high;
    protected decimal low;

    public ReferenceBuilderBase(string valueUnit, string valueCode)
    {
        this.valueUnit = valueUnit;
        this.valueCode = valueCode;
        AppliesTo = new List<Code>();
    }

    public ReferenceBuilderBase High(decimal value)
    {
        this.high = value;
        return this;
    }

    public ReferenceBuilderBase Low(decimal value)
    {
        this.low = value;
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