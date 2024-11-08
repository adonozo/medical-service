namespace QMUL.DiabetesBackend.SeedData.Observations;

using Builders;
using Model;
using Model.Enums;

public static class BloodChemistryTemplateData
{
    public static readonly ICollection<ObservationTemplate> ObservationTemplates = new List<ObservationTemplate>();

    static BloodChemistryTemplateData()
    {
        AddData();
    }

    private static void AddData()
    {
        ObservationTemplates.Add(new ObservationTemplateBuilder()
            .SetType(ObservationType.BloodChemistry)
            .AddCode(
                code: "BC0001",
                display: "Glicemia")
            .SetMgPerDlQuantity()
            .AddReferenceRange((valueUnit, valueCode) =>
                new DecimalValueReferenceBuilder(valueUnit, valueCode)
                    .Low(70)
                    .High(110)
                    .Build())
            .Build());

        ObservationTemplates.Add(new ObservationTemplateBuilder()
            .SetType(ObservationType.BloodChemistry)
            .AddCode(
                code: "BC0002",
                display: "Urea")
            .SetMgPerDlQuantity()
            .AddReferenceRange((valueUnit, valueCode) =>
                new DecimalValueReferenceBuilder(valueUnit, valueCode)
                    .Low(10)
                    .High(50)
                    .Build())
            .Build());

        ObservationTemplates.Add(new ObservationTemplateBuilder()
            .SetType(ObservationType.BloodChemistry)
            .AddCode(
                code: "BC0003",
                display: "Nitrogeno Ureico (N.U.S.")
            .SetMgPerDlQuantity()
            .AddReferenceRange((valueUnit, valueCode) =>
                new DecimalValueReferenceBuilder(valueUnit, valueCode)
                    .Low(8)
                    .High(23)
                    .Build())
            .Build());

        ObservationTemplates.Add(new ObservationTemplateBuilder()
            .SetType(ObservationType.BloodChemistry)
            .AddCode(
                code: "BC0004",
                display: "Creatinina")
            .SetMgPerDlQuantity()
            .AddReferenceRange((valueUnit, valueCode) =>
                new DecimalValueReferenceBuilder(valueUnit, valueCode)
                    .Low(0.7M)
                    .High(1.5M)
                    .Build())
            .Build());

        ObservationTemplates.Add(new ObservationTemplateBuilder()
            .SetType(ObservationType.BloodChemistry)
            .AddCode(
                code: "BC0005",
                display: "Acido Urico")
            .SetMgPerDlQuantity()
            .AddReferenceRange((valueUnit, valueCode) =>
                new DecimalValueReferenceBuilder(valueUnit, valueCode)
                    .Low(3.4M)
                    .High(7.0M)
                    .AppliesToMen()
                    .Build())
            .AddReferenceRange((valueUnit, valueCode) =>
                new DecimalValueReferenceBuilder(valueUnit, valueCode)
                    .Low(2.4M)
                    .High(5.7M)
                    .AppliesToWomen()
                    .Build())
            .Build());

        ObservationTemplates.Add(new ObservationTemplateBuilder()
            .SetType(ObservationType.BloodChemistry)
            .AddCode(
                code: "BC0006",
                display: "Proteinas Totales")
            .SetGPerDlQuantity()
            .AddReferenceRange((valueUnit, valueCode) =>
                new DecimalValueReferenceBuilder(valueUnit, valueCode)
                    .Low(6.6M)
                    .High(8.3M)
                    .Build())
            .Build());

        ObservationTemplates.Add(new ObservationTemplateBuilder()
            .SetType(ObservationType.BloodChemistry)
            .AddCode(
                code: "BC0007",
                display: "Albumina")
            .SetGPerDlQuantity()
            .AddReferenceRange((valueUnit, valueCode) =>
                new DecimalValueReferenceBuilder(valueUnit, valueCode)
                    .Low(3.8M)
                    .High(5.1M)
                    .Build())
            .Build());

        ObservationTemplates.Add(new ObservationTemplateBuilder()
            .SetType(ObservationType.BloodChemistry)
            .AddCode(
                code: "BC0008",
                display: "Globulinas")
            .SetGPerDlQuantity()
            .AddReferenceRange((valueUnit, valueCode) =>
                new DecimalValueReferenceBuilder(valueUnit, valueCode)
                    .Low(1.5M)
                    .High(3.0M)
                    .Build())
            .Build());

        ObservationTemplates.Add(new ObservationTemplateBuilder()
            .SetType(ObservationType.BloodChemistry)
            .AddCode(
                code: "BC0009",
                display: "Relacion Albumina/Globulinas")
            .SetGPerDlQuantity()
            .AddReferenceRange((valueUnit, valueCode) =>
                new DecimalValueReferenceBuilder(valueUnit, valueCode)
                    .Low(1.5M)
                    .High(3.0M)
                    .Build())
            .Build());

        ObservationTemplates.Add(new ObservationTemplateBuilder()
            .SetType(ObservationType.BloodChemistry)
            .AddCode(
                code: "BC0010",
                display: "G.O.T.")
            .SetUPerLQuantity()
            .AddReferenceRange((valueUnit, valueCode) =>
                new DecimalValueReferenceBuilder(valueUnit, valueCode)
                    .Low(8.0M)
                    .High(33.0M)
                    .Build())
            .Build());

        ObservationTemplates.Add(new ObservationTemplateBuilder()
            .SetType(ObservationType.BloodChemistry)
            .AddCode(
                code: "BC0011",
                display: "G.P.T.")
            .SetUPerLQuantity()
            .AddReferenceRange((valueUnit, valueCode) =>
                new DecimalValueReferenceBuilder(valueUnit, valueCode)
                    .Low(3.0M)
                    .High(35.0M)
                    .Build())
            .Build());

        ObservationTemplates.Add(new ObservationTemplateBuilder()
            .SetType(ObservationType.BloodChemistry)
            .AddCode(
                code: "BC0012",
                display: "G.G.T.")
            .SetUPerLQuantity()
            .AddReferenceRange((valueUnit, valueCode) =>
                new DecimalValueReferenceBuilder(valueUnit, valueCode)
                    .Low(0M)
                    .High(5.0M)
                    .AppliesToMen()
                    .Build())
            .AddReferenceRange((valueUnit, valueCode) =>
                new DecimalValueReferenceBuilder(valueUnit, valueCode)
                    .Low(0M)
                    .High(30.0M)
                    .AppliesToWomen()
                    .Build())
            .Build());

        ObservationTemplates.Add(new ObservationTemplateBuilder()
            .SetType(ObservationType.BloodChemistry)
            .AddCode(
                code: "BC0013",
                display: "L.D.H.")
            .SetUPerLQuantity()
            .AddReferenceRange((valueUnit, valueCode) =>
                new DecimalValueReferenceBuilder(valueUnit, valueCode)
                    .Low(0M)
                    .High(243.0M)
                    .AppliesToMen()
                    .Build())
            .AddReferenceRange((valueUnit, valueCode) =>
                new DecimalValueReferenceBuilder(valueUnit, valueCode)
                    .Low(0M)
                    .High(244.0M)
                    .AppliesToWomen()
                    .Build())
            .Build());

        ObservationTemplates.Add(new ObservationTemplateBuilder()
            .SetType(ObservationType.BloodChemistry)
            .AddCode(
                code: "BC0014",
                display: "Bilirrubina Total")
            .SetMgPerDlQuantity()
            .AddReferenceRange((valueUnit, valueCode) =>
                new DecimalValueReferenceBuilder(valueUnit, valueCode)
                    .Low(0.0M)
                    .High(1.1M)
                    .Build())
            .Build());

        ObservationTemplates.Add(new ObservationTemplateBuilder()
            .SetType(ObservationType.BloodChemistry)
            .AddCode(
                code: "BC0015",
                display: "Bilirrubina Directa")
            .SetMgPerDlQuantity()
            .AddReferenceRange((valueUnit, valueCode) =>
                new DecimalValueReferenceBuilder(valueUnit, valueCode)
                    .Low(0.0M)
                    .High(0.25M)
                    .Build())
            .Build());

        ObservationTemplates.Add(new ObservationTemplateBuilder()
            .SetType(ObservationType.BloodChemistry)
            .AddCode(
                code: "BC0016",
                display: "Bilirrubina Indirecta")
            .SetMgPerDlQuantity()
            .AddReferenceRange((valueUnit, valueCode) =>
                new DecimalValueReferenceBuilder(valueUnit, valueCode)
                    .Low(0.0M)
                    .High(1.5M)
                    .Build())
            .Build());

        ObservationTemplates.Add(new ObservationTemplateBuilder()
            .SetType(ObservationType.BloodChemistry)
            .AddCode(
                code: "BC0017",
                display: "Fosfatasa Alcalina")
            .SetUPerLQuantity()
            .AddReferenceRange((valueUnit, valueCode) =>
                new DecimalValueReferenceBuilder(valueUnit, valueCode)
                    .Low(34.0M)
                    .High(114.0M)
                    .Build())
            .Build());

        ObservationTemplates.Add(new ObservationTemplateBuilder()
            .SetType(ObservationType.BloodChemistry)
            .AddCode(
                code: "BC0018",
                display: "Amilasa")
            .SetUPerLQuantity()
            .AddReferenceRange((valueUnit, valueCode) =>
                new DecimalValueReferenceBuilder(valueUnit, valueCode)
                    .Low(16.0M)
                    .High(108.0M)
                    .Build())
            .Build());

        ObservationTemplates.Add(new ObservationTemplateBuilder()
            .SetType(ObservationType.BloodChemistry)
            .AddCode(
                code: "BC0019",
                display: "Colesterol Total")
            .SetMgPerDlQuantity()
            .AddReferenceRange((valueUnit, valueCode) =>
                new DecimalValueReferenceBuilder(valueUnit, valueCode)
                    .Low(120.0M)
                    .High(200.0M)
                    .Build())
            .Build());

        ObservationTemplates.Add(new ObservationTemplateBuilder()
            .SetType(ObservationType.BloodChemistry)
            .AddCode(
                code: "BC0020",
                display: "HDL Colesterol")
            .SetMgPerDlQuantity()
            .AddReferenceRange((valueUnit, valueCode) =>
                new DecimalValueReferenceBuilder(valueUnit, valueCode)
                    .Low(30.0M)
                    .High(70.0M)
                    .Build())
            .Build());

        ObservationTemplates.Add(new ObservationTemplateBuilder()
            .SetType(ObservationType.BloodChemistry)
            .AddCode(
                code: "BC0021",
                display: "LDL Colesterol")
            .SetMgPerDlQuantity()
            .AddReferenceRange((valueUnit, valueCode) =>
                new DecimalValueReferenceBuilder(valueUnit, valueCode)
                    .Low(50.0M)
                    .High(160.0M)
                    .Build())
            .Build());

        ObservationTemplates.Add(new ObservationTemplateBuilder()
            .SetType(ObservationType.BloodChemistry)
            .AddCode(
                code: "BC0022",
                display: "Trigliceridos")
            .SetMgPerDlQuantity()
            .AddReferenceRange((valueUnit, valueCode) =>
                new DecimalValueReferenceBuilder(valueUnit, valueCode)
                    .Low(30.0M)
                    .High(150.0M)
                    .Build())
            .Build());
    }
}