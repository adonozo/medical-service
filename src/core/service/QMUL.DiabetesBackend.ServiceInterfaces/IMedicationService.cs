namespace QMUL.DiabetesBackend.ServiceInterfaces
{
    using System.Threading.Tasks;
    using Hl7.Fhir.Model;

    /// <summary>
    /// The Medication Service Interface.
    /// </summary>
    public interface IMedicationService
    {
        public Task<Bundle> GetMedicationList();

        public Task<Medication> GetSingleMedication(string id);

        public Task<Medication> CreateMedication(Medication newMedication);
    }
}