namespace QMUL.DiabetesBackend.SeedData.Observations;

using Builders;
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

        ObservationTemplates.Add(new ObservationTemplateBuilder()
            .SetType(ObservationType.Hemogram)
            .AddCode(
                code: "HM0020",
                display: "V.C.M.")
            .SetFLQuantity()
            .AddReferenceRange((valueUnit, valueCode) =>
                new DecimalValueReferenceBuilder(valueUnit, valueCode)
                    .Low(79)
                    .High(83)
                    .Build())
            .Build());

        ObservationTemplates.Add(new ObservationTemplateBuilder()
            .SetType(ObservationType.Hemogram)
            .AddCode(
                code: "HM0021",
                display: "H.C.M.")
            .SetPGQuantity()
            .AddReferenceRange((valueUnit, valueCode) =>
                new DecimalValueReferenceBuilder(valueUnit, valueCode)
                    .Low(27)
                    .High(31)
                    .Build())
            .Build());

        ObservationTemplates.Add(new ObservationTemplateBuilder()
            .SetType(ObservationType.Hemogram)
            .AddCode(
                code: "HM0022",
                display: "C.H.C.M.")
            .SetGPerDlQuantity()
            .AddReferenceRange((valueUnit, valueCode) =>
                new DecimalValueReferenceBuilder(valueUnit, valueCode)
                    .Low(32)
                    .High(36)
                    .Build())
            .Build());

        ObservationTemplates.Add(new ObservationTemplateBuilder()
            .SetType(ObservationType.Hemogram)
            .AddCode(
                code: "HM0023",
                display: "Recuento de Plaquetas")
            .SetPerMm3Quantity()
            .AddReferenceRange((valueUnit, valueCode) =>
                new DecimalValueReferenceBuilder(valueUnit, valueCode)
                    .Low(150_000)
                    .High(400_000)
                    .Build())
            .Build());

        ObservationTemplates.Add(new ObservationTemplateBuilder()
            .SetType(ObservationType.Hemogram)
            .AddCode(
                code: "HM0024",
                display: "Recuento de Reticulocitos")
            .SetPercentageQuantity()
            .AddReferenceRange((valueUnit, valueCode) =>
                new DecimalValueReferenceBuilder(valueUnit, valueCode)
                    .Low(0.5M)
                    .High(2.0M)
                    .Build())
            .Build());

        ObservationTemplates.Add(new ObservationTemplateBuilder()
            .SetType(ObservationType.Hemogram)
            .AddCode(
                code: "HM0025",
                display: "Tiempo de sangría")
            .SetMinutesQuantity()
            .AddReferenceRange((valueUnit, valueCode) =>
                new DecimalValueReferenceBuilder(valueUnit, valueCode)
                    .Low(1)
                    .High(3)
                    .Build())
            .Build());

        ObservationTemplates.Add(new ObservationTemplateBuilder()
            .SetType(ObservationType.Hemogram)
            .AddCode(
                code: "HM0026",
                display: "Tiempo de coagulación")
            .SetMinutesQuantity()
            .AddReferenceRange((valueUnit, valueCode) =>
                new DecimalValueReferenceBuilder(valueUnit, valueCode)
                    .Low(5)
                    .High(10)
                    .Build())
            .Build());

        ObservationTemplates.Add(new ObservationTemplateBuilder()
            .SetType(ObservationType.Hemogram)
            .AddCode(
                code: "HM0027",
                display: "Tiempo de protombina")
            .SetSecondsQuantity()
            .AddReferenceRange((valueUnit, valueCode) =>
                new DecimalValueReferenceBuilder(valueUnit, valueCode)
                    .Low(12)
                    .High(12)
                    .Build())
            .Build());

        ObservationTemplates.Add(new ObservationTemplateBuilder()
            .SetType(ObservationType.Hemogram)
            .AddCode(
                code: "HM0028",
                display: "Tiempo de protombina")
            .SetSecondsQuantity()
            .AddReferenceRange((valueUnit, valueCode) =>
                new DecimalValueReferenceBuilder(valueUnit, valueCode)
                    .Low(12)
                    .High(12)
                    .Build())
            .Build());

        ObservationTemplates.Add(new ObservationTemplateBuilder()
            .SetType(ObservationType.Hemogram)
            .AddCode(
                code: "HM0029",
                display: "Actividad")
            .Build());

        ObservationTemplates.Add(new ObservationTemplateBuilder()
            .SetType(ObservationType.Hemogram)
            .AddCode(
                code: "HM0030",
                display: "INR")
            .Build());

        ObservationTemplates.Add(new ObservationTemplateBuilder()
            .SetType(ObservationType.Hemogram)
            .AddCode(
                code: "HM0031",
                display: "TPA")
            .SetSecondsQuantity()
            .AddReferenceRange((valueUnit, valueCode) =>
                new DecimalValueReferenceBuilder(valueUnit, valueCode)
                    .Low(33)
                    .High(48)
                    .Build())
            .Build());

        ObservationTemplates.Add(new ObservationTemplateBuilder()
            .SetType(ObservationType.Hemogram)
            .AddCode(
                code: "HM0040",
                display: "Grupo Sanguieno")
            .Build());

        ObservationTemplates.Add(new ObservationTemplateBuilder()
            .SetType(ObservationType.Hemogram)
            .AddCode(
                code: "HM0041",
                display: "Factor RH")
            .Build());
    }
}