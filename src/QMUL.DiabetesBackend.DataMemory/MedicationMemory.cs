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

        public MedicationMemory()
        {
            this.medications = new List<Medication>();
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