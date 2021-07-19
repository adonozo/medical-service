using Hl7.Fhir.Model;

namespace QMUL.DiabetesBackend.ServiceInterfaces
{
    public interface IMedicationRequestService
    {
        public MedicationRequest CreateMedicationRequest(MedicationRequest request);

        public MedicationRequest GetMedicationRequest(string id);

        public MedicationRequest UpdateMedicationRequest(string id, MedicationRequest request);

        public bool DeleteMedicationRequest(string id);
    }
}