using System;
using System.Collections.Generic;
using System.Linq;
using Hl7.Fhir.Model;
using QMUL.DiabetesBackend.DataInterfaces;

namespace QMUL.DiabetesBackend.DataMemory
{
    public class MedicationMemory : IMedicationDao
    {
        #region SampleData
        private readonly List<Hl7.Fhir.Model.Medication> sampleMedications = new() { };

        public static List<Hl7.Fhir.Model.Medication> Medications = new()
        {
            new()
            {
                Id = "d7b222dd-4a53-45ed-803b-4c47a05390aa",
                Code = new CodeableConcept
                {
                    Coding = new List<Coding>
                    {
                        new()
                        {
                            System = "http://hl7.org/fhir/sid/ndc",
                            Code = "0169-7501-11",
                            Display = "Novolog 100u/ml"
                        }
                    }
                },
                Form = new CodeableConcept
                {
                    Coding = new List<Coding>
                    {
                        new()
                        {
                            System = "http://snomed.info/sct",
                            Code = "385219001",
                            Display = "Injection solution (qualifier value)"
                        }
                    }
                },
                Ingredient = new List<Hl7.Fhir.Model.Medication.IngredientComponent>
                {
                    new()
                    {
                        Item = new CodeableConcept
                        {
                            Coding = new List<Coding>
                            {
                                new Coding
                                {
                                    System = "http://snomed.info/sct",
                                    Code = "325072002",
                                    Display = "Insulin Aspart (substance)"
                                }
                            }
                        },
                        Strength = new Ratio
                        {
                            Numerator = new Quantity
                            {
                                Value = 100,
                                System = "http://unitsofmeasure.org",
                                Unit = "U"
                            },
                            Denominator = new Quantity
                            {
                                Value = 1,
                                System = "http://unitsofmeasure.org",
                                Unit = "mL"
                            }
                        }
                    }
                }
            }
        };
        #endregion

        public MedicationMemory()
        {
            this.sampleMedications = Medications;
        }

        public List<Medication> GetMedicationList()
        {
            return this.sampleMedications;
        }

        public Medication GetSingleMedication(Guid id)
        {
            return this.sampleMedications.FirstOrDefault(medication => medication.Id.Equals(id));
        }
    }
}