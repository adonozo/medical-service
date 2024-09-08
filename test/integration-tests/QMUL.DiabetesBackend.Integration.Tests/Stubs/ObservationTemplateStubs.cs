namespace QMUL.DiabetesBackend.Integration.Tests.Stubs;

using Model;
using Model.Enums;
using Model.FHIR;

public static class ObservationTemplateStubs
{
    public static readonly ObservationTemplate GlucoseTemplate = new()
    {
        Code = new Code(
            Coding: new Coding(
                System: "http://loinc.org",
                Code: "15074-8",
                Display: "Glucose [Moles/volume] in Blood")),
        ReferenceRange = new []
        {
            new Reference(
                Low: new ValueQuantity(
                    Unit: "mmol/l",
                    System: "http://unitsofmeasure.org",
                    Code: "mmol/L"),
                High: new ValueQuantity(
                    Unit: "mmol/l",
                    System: "http://unitsofmeasure.org",
                    Code: "mmol/L"))
        },
        ValueTemplate = new ValueQuantity(
            Unit: "mmol/l",
            System: "http://unitsofmeasure.org",
            Code: "mmol/L"),
        Metadata = new ObservationMetadata
        {
            ObservationType = ObservationType.BloodChemistry,
        }
    };
}