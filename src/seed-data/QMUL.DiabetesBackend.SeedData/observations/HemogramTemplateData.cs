namespace QMUL.DiabetesBackend.SeedData.observations;

using Model;
using Model.Enums;
using Model.FHIR;

public static class HemogramTemplateData
{
    public static readonly ICollection<ObservationTemplate> ObservationTemplates = new List<ObservationTemplate>();

    static HemogramTemplateData()
    {
        AddData();
    }

    private static void AddData()
    {
        ObservationTemplates.Add(new ObservationTemplateBuilder()
            .SetType(ObservationType.Hemogram)
            .AddCode(
                code: "HM0001",
                display: "Globulos Rojos")
            .SetMm3ValueQuantity()
            .AddReferences(
                new Reference(
                    Low: new DecimalValueQuantity(
                        Unit: "mm3",
                        System: "",
                        Code: "",
                        Value: 5_390_000),
                    High: new DecimalValueQuantity(
                        Unit: "mm3",
                        System: "",
                        Code: "",
                        Value: 6_270_000),
                    AppliesTo: new List<Code>
                    {
                        new(new Coding(
                            System: "",
                            Code: "",
                            Display: "Varon"))
                    }),
                new Reference(
                    Low: new DecimalValueQuantity(
                        Unit: "mm3",
                        System: "",
                        Code: "",
                        Value: 4_800_000),
                    High: new DecimalValueQuantity(
                        Unit: "mm3",
                        System: "",
                        Code: "",
                        Value: 5_900_000),
                    AppliesTo: new List<Code>
                    {
                        new(new Coding(
                            System: "",
                            Code: "",
                            Display: "Mujer"))
                    }))
            .Build());

        ObservationTemplates.Add(new ObservationTemplateBuilder()
            .SetType(ObservationType.Hemogram)
            .AddCode(
                code: "HM0002",
                display: "Globulos Blancos")
            .SetMm3ValueQuantity()
            .AddReferences(new Reference(
                Low: new DecimalValueQuantity(
                    Unit: "mm3",
                    System: "",
                    Code: "",
                    Value: 5_000),
                High: new DecimalValueQuantity(
                    Unit: "mm3",
                    System: "",
                    Code: "",
                    Value: 8_000),
                AppliesTo: new List<Code>
                {
                    new(new Coding(
                        System: "",
                        Code: "",
                        Display: "Varon y Mujer"))
                }))
            .Build());

        ObservationTemplates.Add(new ObservationTemplateBuilder()
            .SetType(ObservationType.Hemogram)
            .AddCode(
                code: "HM0003",
                display: "Hematocrito")
            .SetPercentageQuantity()
            .AddReferences(new Reference(
                    Low: new DecimalValueQuantity(
                        Unit: "%",
                        System: "",
                        Code: "",
                        Value: 47),
                    High: new DecimalValueQuantity(
                        Unit: "%",
                        System: "",
                        Code: "",
                        Value: 49),
                    AppliesTo: new List<Code>
                    {
                        new(new Coding(
                            System: "",
                            Code: "",
                            Display: "Varon"))
                    }),
                new Reference(
                    Low: new DecimalValueQuantity(
                        Unit: "%",
                        System: "",
                        Code: "",
                        Value: 44),
                    High: new DecimalValueQuantity(
                        Unit: "%",
                        System: "",
                        Code: "",
                        Value: 53),
                    AppliesTo: new List<Code>
                    {
                        new(new Coding(
                            System: "",
                            Code: "",
                            Display: "Mujer"))
                    }))
            .Build());

        ObservationTemplates.Add(new ObservationTemplateBuilder()
            .SetType(ObservationType.Hemogram)
            .AddCode(
                code: "HM0004",
                display: "Hemoglobina")
            .SetGPerDlQuantity()
            .AddReferences(new Reference(
                    Low: new DecimalValueQuantity(
                        Unit: "g/dL",
                        System: "",
                        Code: "",
                        Value: 16.9M),
                    High: new DecimalValueQuantity(
                        Unit: "g/dL",
                        System: "",
                        Code: "",
                        Value: 18.4M),
                    AppliesTo: new List<Code>
                    {
                        new(new Coding(
                            System: "",
                            Code: "",
                            Display: "Varon"))
                    }),
                new Reference(
                    Low: new DecimalValueQuantity(
                        Unit: "g/dL",
                        System: "",
                        Code: "",
                        Value: 14.4M),
                    High: new DecimalValueQuantity(
                        Unit: "g/dL",
                        System: "",
                        Code: "",
                        Value: 17.4M),
                    AppliesTo: new List<Code>
                    {
                        new(new Coding(
                            System: "",
                            Code: "",
                            Display: "Mujer"))
                    }))
            .Build());

        ObservationTemplates.Add(new ObservationTemplateBuilder()
            .SetType(ObservationType.Hemogram)
            .AddCode(
                code: "HM0005",
                display: "VES 1a Hora")
            .SetMmhQuantity()
            .Build());

        ObservationTemplates.Add(new ObservationTemplateBuilder()
            .SetType(ObservationType.Hemogram)
            .AddCode(
                code: "HM0006",
                display: "VES 2a Hora")
            .SetMmhQuantity()
            .Build());

        ObservationTemplates.Add(new ObservationTemplateBuilder()
            .SetType(ObservationType.Hemogram)
            .AddCode(
                code: "HM0007",
                display: "Indice de Kats")
            .Build());

        ObservationTemplates.Add(new ObservationTemplateBuilder()
            .SetType(ObservationType.Hemogram)
            .AddCode(
                code: "HM0008",
                display: "Basinofilos")
            .SetPercentageQuantity()
            .AddReferences(new Reference(
                    Low: new DecimalValueQuantity(
                        Unit: "%",
                        System: "",
                        Code: "",
                        Value: 0),
                    High: new DecimalValueQuantity(
                        Unit: "%",
                        System: "",
                        Code: "",
                        Value: 1)))
            .Build());

        ObservationTemplates.Add(new ObservationTemplateBuilder()
            .SetType(ObservationType.Hemogram)
            .AddCode(
                code: "HM0009",
                display: "Eosinofilos")
            .SetPercentageQuantity()
            .AddReferences(new Reference(
                Low: new DecimalValueQuantity(
                    Unit: "%",
                    System: "",
                    Code: "",
                    Value: 0),
                High: new DecimalValueQuantity(
                    Unit: "%",
                    System: "",
                    Code: "",
                    Value: 4)))
            .Build());

        ObservationTemplates.Add(new ObservationTemplateBuilder()
            .SetType(ObservationType.Hemogram)
            .AddCode(
                code: "HM0010",
                display: "Cayados")
            .SetPercentageQuantity()
            .AddReferences(new Reference(
                Low: new DecimalValueQuantity(
                    Unit: "%",
                    System: "",
                    Code: "",
                    Value: 0),
                High: new DecimalValueQuantity(
                    Unit: "%",
                    System: "",
                    Code: "",
                    Value: 3)))
            .Build());

        ObservationTemplates.Add(new ObservationTemplateBuilder()
            .SetType(ObservationType.Hemogram)
            .AddCode(
                code: "HM0011",
                display: "Segmentados")
            .SetPercentageQuantity()
            .AddReferences(new Reference(
                Low: new DecimalValueQuantity(
                    Unit: "%",
                    System: "",
                    Code: "",
                    Value: 55),
                High: new DecimalValueQuantity(
                    Unit: "%",
                    System: "",
                    Code: "",
                    Value: 65)))
            .Build());

        ObservationTemplates.Add(new ObservationTemplateBuilder()
            .SetType(ObservationType.Hemogram)
            .AddCode(
                code: "HM0012",
                display: "Linfocitos")
            .SetPercentageQuantity()
            .AddReferences(new Reference(
                Low: new DecimalValueQuantity(
                    Unit: "%",
                    System: "",
                    Code: "",
                    Value: 25),
                High: new DecimalValueQuantity(
                    Unit: "%",
                    System: "",
                    Code: "",
                    Value: 35)))
            .Build());

        ObservationTemplates.Add(new ObservationTemplateBuilder()
            .SetType(ObservationType.Hemogram)
            .AddCode(
                code: "HM0013",
                display: "Monocitos")
            .SetPercentageQuantity()
            .AddReferences(new Reference(
                Low: new DecimalValueQuantity(
                    Unit: "%",
                    System: "",
                    Code: "",
                    Value: 2),
                High: new DecimalValueQuantity(
                    Unit: "%",
                    System: "",
                    Code: "",
                    Value: 6)))
            .Build());

        ObservationTemplates.Add(new ObservationTemplateBuilder()
            .SetType(ObservationType.Hemogram)
            .AddCode(
                code: "HM0014",
                display: "Otros")
            .SetValueQuantity(new ValueQuantity(
                Unit: "",
                System: "",
                Code: ""))
            .Build());
    }
}