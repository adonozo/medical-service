namespace QMUL.DiabetesBackend.SeedData.observations;

using Model;
using Model.Enums;
using Model.FHIR;

public class ObservationTemplateBuilder
{
    private readonly ObservationTemplate template;

    public ObservationTemplateBuilder()
    {
        this.template = new ObservationTemplate();
    }

    public ObservationTemplateBuilder SetType(ObservationType type)
    {
        this.template.Metadata = new ObservationMetadata
        {
            ObservationType = type
        };

        return this;
    }

    public ObservationTemplateBuilder AddCode(string code, string display)
    {
        this.template.Code = new Code(new Coding(
            System: CustomCodes.ObservationsSystem,
            Code: code,
            Display: display));
        return this;
    }

    public ObservationTemplateBuilder SetValueQuantity(ValueQuantity valueQuantity)
    {
        this.template.ValueTemplate = valueQuantity;
        return this;
    }

    public ObservationTemplateBuilder SetMm3ValueQuantity()
    {
        this.template.ValueTemplate = new ValueQuantity(
            Unit: "mm3",
            System: "",
            Code: "");
        return this;
    }

    public ObservationTemplateBuilder SetPercentageQuantity()
    {
        this.template.ValueTemplate = new ValueQuantity(
            Unit: "%",
            System: "",
            Code: "");
        return this;
    }

    public ObservationTemplateBuilder SetGPerDlQuantity()
    {
        this.template.ValueTemplate = new ValueQuantity(
            Unit: "g/dL",
            System: "",
            Code: "");
        return this;
    }

    public ObservationTemplateBuilder SetMmhQuantity()
    {
        this.template.ValueTemplate = new ValueQuantity(
            Unit: "mm/H",
            System: "",
            Code: "");
        return this;
    }

    public ObservationTemplateBuilder AddReferences(params Reference[] references)
    {
        this.template.ReferenceRange = references;
        return this;
    }

    public ObservationTemplate Build() => this.template;
}