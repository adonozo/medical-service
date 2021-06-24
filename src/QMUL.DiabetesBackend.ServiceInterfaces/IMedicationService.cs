using System.Collections.Generic;
using QMUL.DiabetesBackend.Model;

namespace QMUL.DiabetesBackend.ServiceInterfaces
{
    public interface IMedicationService
    {
        public List<Medication> GetMedicationList();
    }
}