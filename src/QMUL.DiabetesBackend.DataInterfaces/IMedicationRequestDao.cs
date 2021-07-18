using Hl7.Fhir.Model;

namespace QMUL.DiabetesBackend.DataInterfaces
{
    public interface IMedicationRequestDao
    {
        public MedicationRequest CreatMedicationRequest(MedicationRequest newRequest);

        public MedicationRequest UpdateMedicationRequest(string id, MedicationRequest actualRequest);

        public MedicationRequest GetMedicationRequest(string id);

        public bool DeleteMedicationRequest(string id);
    }
}