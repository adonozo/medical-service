using System.Collections.Generic;
using QMUL.DiabetesBackend.Model;

namespace QMUL.DiabetesBackend.Service
{
    public interface IMedicationService
    {
        public List<Medication> GetMedicationList();
    }
}