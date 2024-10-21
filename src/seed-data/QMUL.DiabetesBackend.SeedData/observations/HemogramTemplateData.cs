namespace QMUL.DiabetesBackend.SeedData.observations;

using Model;
using Model.Enums;

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
            .AddReferenceRange((valueUnit, valueCode) =>
                new DecimalValueReferenceBuilder(valueUnit, valueCode)
                    .Low(5_390_000)
                    .High(6_270_000)
                    .AppliesToMen()
                    .Build())
            .AddReferenceRange((valueUnit, valueCode) =>
                new DecimalValueReferenceBuilder(valueUnit, valueCode)
                    .Low(4_800_000)
                    .High(5_900_000)
                    .AppliesToWomen()
                    .Build())
            .Build());

        ObservationTemplates.Add(new ObservationTemplateBuilder()
            .SetType(ObservationType.Hemogram)
            .AddCode(
                code: "HM0002",
                display: "Globulos Blancos")
            .SetMm3ValueQuantity()
            .AddReferenceRange((valueUnit, valueCode) =>
                new DecimalValueReferenceBuilder(valueUnit, valueCode)
                    .Low(5_000)
                    .High(8_000)
                    .AppliesToMen()
                    .AppliesToWomen()
                    .Build())
            .Build());

        ObservationTemplates.Add(new ObservationTemplateBuilder()
            .SetType(ObservationType.Hemogram)
            .AddCode(
                code: "HM0003",
                display: "Hematocrito")
            .SetPercentageQuantity()
            .AddReferenceRange((valueUnit, valueCode) =>
                new DecimalValueReferenceBuilder(valueUnit, valueCode)
                    .Low(47)
                    .High(49)
                    .AppliesToMen()
                    .Build())
            .AddReferenceRange((valueUnit, valueCode) =>
                new DecimalValueReferenceBuilder(valueUnit, valueCode)
                    .Low(44)
                    .High(53)
                    .AppliesToWomen()
                    .Build())
            .Build());

        ObservationTemplates.Add(new ObservationTemplateBuilder()
            .SetType(ObservationType.Hemogram)
            .AddCode(
                code: "HM0004",
                display: "Hemoglobina")
            .SetGPerDlQuantity()
            .AddReferenceRange((valueUnit, valueCode) =>
                new DecimalValueReferenceBuilder(valueUnit, valueCode)
                    .Low(16.9M)
                    .High(18.4M)
                    .AppliesToMen()
                    .Build())
            .AddReferenceRange((valueUnit, valueCode) =>
                new DecimalValueReferenceBuilder(valueUnit, valueCode)
                    .Low(14.4M)
                    .High(17.4M)
                    .AppliesToWomen()
                    .Build())
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
            .AddReferenceRange((valueUnit, valueCode) =>
                new DecimalValueReferenceBuilder(valueUnit, valueCode)
                    .Low(0)
                    .High(1)
                    .Build())
            .Build());

        ObservationTemplates.Add(new ObservationTemplateBuilder()
            .SetType(ObservationType.Hemogram)
            .AddCode(
                code: "HM0009",
                display: "Eosinofilos")
            .SetPercentageQuantity()
            .AddReferenceRange((valueUnit, valueCode) =>
                new DecimalValueReferenceBuilder(valueUnit, valueCode)
                    .Low(0)
                    .High(4)
                    .Build())
            .Build());

        ObservationTemplates.Add(new ObservationTemplateBuilder()
            .SetType(ObservationType.Hemogram)
            .AddCode(
                code: "HM0010",
                display: "Cayados")
            .SetPercentageQuantity()
            .AddReferenceRange((valueUnit, valueCode) =>
                new DecimalValueReferenceBuilder(valueUnit, valueCode)
                    .Low(0)
                    .High(3)
                    .Build())
            .Build());

        ObservationTemplates.Add(new ObservationTemplateBuilder()
            .SetType(ObservationType.Hemogram)
            .AddCode(
                code: "HM0011",
                display: "Segmentados")
            .SetPercentageQuantity()
            .AddReferenceRange((valueUnit, valueCode) =>
                new DecimalValueReferenceBuilder(valueUnit, valueCode)
                    .Low(55)
                    .High(65)
                    .Build())
            .Build());

        ObservationTemplates.Add(new ObservationTemplateBuilder()
            .SetType(ObservationType.Hemogram)
            .AddCode(
                code: "HM0012",
                display: "Linfocitos")
            .SetPercentageQuantity()
            .AddReferenceRange((valueUnit, valueCode) =>
                new DecimalValueReferenceBuilder(valueUnit, valueCode)
                    .Low(25)
                    .High(35)
                    .Build())
            .Build());

        ObservationTemplates.Add(new ObservationTemplateBuilder()
            .SetType(ObservationType.Hemogram)
            .AddCode(
                code: "HM0013",
                display: "Monocitos")
            .SetPercentageQuantity()
            .AddReferenceRange((valueUnit, valueCode) =>
                new DecimalValueReferenceBuilder(valueUnit, valueCode)
                    .Low(2)
                    .High(6)
                    .Build())
            .Build());
    }
}