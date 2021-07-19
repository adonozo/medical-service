using System;
using System.Collections.Generic;
using Hl7.Fhir.Model;

namespace QMUL.DiabetesBackend.DataInterfaces
{
    public interface IMedicationDao
    {
        public List<Medication> GetMedicationList();

        public Medication GetSingleMedication(Guid id);
    }
}