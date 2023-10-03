namespace QMUL.DiabetesBackend.Integration.Tests.Stubs;

using System.Collections.Generic;
using Hl7.Fhir.Model;

public static class MedicationStubs
{
    public static readonly Medication Lorazepam = new()
    {
        Code = new CodeableConcept
        {
            Coding = new List<Coding>
            {
                new()
                {
                    Code = "400621001",
                    System = "http://snomed.info/sct",
                    Display = "Lorazepam 2mg/ml injection solution 1ml vial (product)"
                }
            }
        },
        DoseForm = new CodeableConcept
        {
            Coding = new List<Coding>
            {
                new()
                {
                    Code = "385219001",
                    System = "http://snomed.info/sct",
                    Display = "Injection solution (qualifier value)"
                }
            }
        },
        Ingredient = new List<Medication.IngredientComponent>
        {
            new()
            {
                Item = new CodeableReference
                {
                    Concept = new CodeableConcept
                    {
                        Coding = new List<Coding>
                        {
                            new()
                            {
                                Code = "387106007",
                                System = "http://snomed.info/sct",
                                Display = "Lorazepam (substance)"
                            }
                        }
                    }
                },
                Strength = new Ratio
                {
                    Numerator = new Quantity
                    {
                        Code = "mg",
                        Value = 2,
                        System = "http://unitsofmeasure.org"
                    },
                    Denominator = new Quantity
                    {
                        Code = "mL",
                        Value = 1,
                        System = "http://unitsofmeasure.org"
                    }
                }
            }
        }
    };
}