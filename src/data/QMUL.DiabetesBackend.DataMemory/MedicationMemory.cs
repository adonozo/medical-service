using System.Collections.Generic;
using System.Linq;
using Hl7.Fhir.Model;
using QMUL.DiabetesBackend.DataInterfaces;

namespace QMUL.DiabetesBackend.DataMemory
{
    public class MedicationMemory : IMedicationDao
    {
        #region SampleData
        private readonly List<Medication> sampleMedications;

        public static readonly List<Medication> Medications = new()
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
                Ingredient = new List<Medication.IngredientComponent>
                {
                    new()
                    {
                        Item = new CodeableConcept
                        {
                            Coding = new List<Coding>
                            {
                                new()
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
            sampleMedications = Medications;
        }

        public List<Medication> GetMedicationList()
        {
            return sampleMedications;
        }

        public Medication GetSingleMedication(string id)
        {
            return sampleMedications.FirstOrDefault(medication => medication.Id.Equals(id));
        }
    }
}