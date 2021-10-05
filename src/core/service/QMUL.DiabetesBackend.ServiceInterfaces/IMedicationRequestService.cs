namespace QMUL.DiabetesBackend.ServiceInterfaces
{
    using System.Threading.Tasks;
    using Hl7.Fhir.Model;

    /// <summary>
    /// The Medication Request Service Interface.
    /// </summary>
    public interface IMedicationRequestService
    {
        public Task<MedicationRequest> CreateMedicationRequest(MedicationRequest request);

        public Task<MedicationRequest> GetMedicationRequest(string id);

        public Task<MedicationRequest> UpdateMedicationRequest(string id, MedicationRequest request);

        public Task<bool> DeleteMedicationRequest(string id);
        
        public Task<Bundle> GetActiveMedicationRequests(string patientIdOrEmail);
    }
}