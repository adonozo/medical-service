namespace QMUL.DiabetesBackend.DataInterfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Hl7.Fhir.Model;

    /// <summary>
    /// The Medication Dao interface.
    /// </summary>
    public interface IMedicationDao
    {
        public Task<List<Medication>> GetMedicationList();

        public Task<Medication> GetSingleMedication(string id);

        public Task<Medication> CreateMedication(Medication newMedication);
    }
}