using System.Collections.Generic;
using Hl7.Fhir.Model;

namespace QMUL.DiabetesBackend.ServiceInterfaces
{
    public interface IMedicationService
    {
        public List<Medication> GetMedicationList();
    }
}