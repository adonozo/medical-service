using System.Collections.Generic;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using Task = Hl7.Fhir.Model.Task;

namespace QMUL.DiabetesBackend.ServiceInterfaces
{
    public interface IMedicationService
    {
        public Task<Bundle> GetMedicationList();

        public Task<Medication> GetSingleMedication(string id);

        public Task<Medication> CreateMedication(Medication newMedication);
    }
}