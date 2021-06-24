using System;
using System.Collections.Generic;
using System.Linq;
using QMUL.DiabetesBackend.DataInterfaces;
using QMUL.DiabetesBackend.Model;

namespace QMUL.DiabetesBackend.DataMemory
{
    public class MedicationMemory : IMedicationDao
    {
        private readonly List<Medication> medications;

        private readonly List<Medication> sampleMedications = new()
        {
            new Medication
            {
                Id = Guid.Parse("92be6243-8e85-46b1-aa96-c9948ebbed99"),
                Name = "Exenatide",
                Description = "Binds to, and activates, the GLP-1 receptor to increase insulin secretion, suppresses glucagon secretion, and slows gastric emptying.",
                DrugAdministrationRoute = "Injection",
                BrandNames = new List<string> {"Byetta", "Bydureon"}
            }
        };

        public MedicationMemory()
        {
            this.medications = sampleMedications;
        }

        public List<Medication> GetMedicationList()
        {
            return this.medications;
        }

        public Medication GetSingleMedication(Guid id)
        {
            return this.medications.FirstOrDefault(medication => medication.Id.Equals(id));
        }
    }
}