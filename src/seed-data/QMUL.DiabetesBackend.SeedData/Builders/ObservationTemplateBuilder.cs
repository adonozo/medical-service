namespace QMUL.DiabetesBackend.SeedData.Builders;

using System;
using System.Collections.Generic;
using Model;
using Model.FHIR;

public class ObservationTemplateBuilder
{
    private readonly ObservationTemplate template;

    public ObservationTemplateBuilder()
    {
        this.template = new ObservationTemplate
        {
            ReferenceRange = new List<ReferenceValueRange>(),
            Metadata = new ObservationMetadata()
        };
    }

    public ObservationTemplateBuilder InitialVersion()
    {
        this.template.Metadata.Version = 1;
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

    public ObservationTemplateBuilder SetMm3ValueQuantity()
    {
        this.template.ValueTemplate = new ValueQuantity(
            Unit: "mm3",
            System: CustomCodes.UnitsSystem,
            Code: "");
        return this;
    }

    public ObservationTemplateBuilder SetPercentageQuantity()
    {
        this.template.ValueTemplate = new ValueQuantity(
            Unit: "%",
            System: CustomCodes.UnitsSystem,
            Code: "");
        return this;
    }

    public ObservationTemplateBuilder SetGPerDlQuantity()
    {
        this.template.ValueTemplate = new ValueQuantity(
            Unit: "g/dL",
            System: CustomCodes.UnitsSystem,
            Code: "");
        return this;
    }

    public ObservationTemplateBuilder SetMgPerDlQuantity()
    {
        this.template.ValueTemplate = new ValueQuantity(
            Unit: "mg/dL",
            System: CustomCodes.UnitsSystem,
            Code: "");
        return this;
    }

    public ObservationTemplateBuilder SetMmhQuantity()
    {
        this.template.ValueTemplate = new ValueQuantity(
            Unit: "mm/H",
            System: CustomCodes.UnitsSystem,
            Code: "");
        return this;
    }

    public ObservationTemplateBuilder SetPerMm3Quantity()
    {
        this.template.ValueTemplate = new ValueQuantity(
            Unit: "/mm3",
            System: CustomCodes.UnitsSystem,
            Code: "");
        return this;
    }

    public ObservationTemplateBuilder SetUPerLQuantity()
    {
        this.template.ValueTemplate = new ValueQuantity(
            Unit: "U/L",
            System: CustomCodes.UnitsSystem,
            Code: "");
        return this;
    }

    public ObservationTemplateBuilder SetFLQuantity()
    {
        this.template.ValueTemplate = new ValueQuantity(
            Unit: "fL",
            System: CustomCodes.UnitsSystem,
            Code: "");
        return this;
    }

    public ObservationTemplateBuilder SetPGQuantity()
    {
        this.template.ValueTemplate = new ValueQuantity(
            Unit: "pg",
            System: CustomCodes.UnitsSystem,
            Code: "");
        return this;
    }

    public ObservationTemplateBuilder SetMinutesQuantity()
    {
        this.template.ValueTemplate = new ValueQuantity(
            Unit: "min",
            System: CustomCodes.UnitsSystem,
            Code: "");
        return this;
    }

    public ObservationTemplateBuilder SetSecondsQuantity()
    {
        this.template.ValueTemplate = new ValueQuantity(
            Unit: "sec",
            System: CustomCodes.UnitsSystem,
            Code: "");
        return this;
    }

    public ObservationTemplateBuilder AddReferenceRange(Func<string, string, ReferenceValueRange> referenceFunc)
    {
        ArgumentNullException.ThrowIfNull(this.template.ValueTemplate);
        this.template.ReferenceRange.Add(
            referenceFunc.Invoke(this.template.ValueTemplate.Unit, this.template.ValueTemplate.Code));
        return this;
    }

    public ObservationTemplate Build() => this.template;
}