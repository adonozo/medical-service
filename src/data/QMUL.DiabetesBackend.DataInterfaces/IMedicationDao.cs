using System.Collections.Generic;
using System.Threading.Tasks;
using Hl7.Fhir.Model;

namespace QMUL.DiabetesBackend.DataInterfaces
{
    public interface IMedicationDao
    {
        public Task<List<Medication>> GetMedicationList();

        public Task<Medication> GetSingleMedication(string id);

        public Task<Medication> CreateMedication(Medication newMedication);
    }
}