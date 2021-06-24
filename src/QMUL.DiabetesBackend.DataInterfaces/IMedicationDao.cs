using System;
using System.Collections.Generic;
using QMUL.DiabetesBackend.Model;

namespace QMUL.DiabetesBackend.DataInterfaces
{
    public interface IMedicationDao
    {
        public List<Medication> GetMedicationList();

        public Medication GetSingleMedication(Guid id);
    }
}