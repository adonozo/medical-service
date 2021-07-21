using System.Collections.Generic;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using QMUL.DiabetesBackend.DataInterfaces;
using QMUL.DiabetesBackend.ServiceInterfaces;

namespace QMUL.DiabetesBackend.ServiceImpl.Implementations
{
    public class MedicationRequestService : IMedicationRequestService
    {
        private readonly IMedicationRequestDao medicationRequestDao;

        public MedicationRequestService(IMedicationRequestDao medicationRequestDao)
        {
            this.medicationRequestDao = medicationRequestDao;
        }

        public Task<MedicationRequest> CreateMedicationRequest(MedicationRequest request)
        {
            return this.medicationRequestDao.CreateMedicationRequest(request);
        }

        public Task<MedicationRequest> GetMedicationRequest(string id)
        {
            return this.medicationRequestDao.GetMedicationRequest(id);
        }

        public Task<MedicationRequest> UpdateMedicationRequest(string id, MedicationRequest request)
        {
            var exists = this.medicationRequestDao.GetMedicationRequest(id) != null;
            if (exists)
            {
                return this.medicationRequestDao.UpdateMedicationRequest(id, request);
            }

            throw new KeyNotFoundException();
        }

        public Task<bool> DeleteMedicationRequest(string id)
        {
            var exists = this.medicationRequestDao.GetMedicationRequest(id) != null;
            if (exists)
            {
                return this.medicationRequestDao.DeleteMedicationRequest(id);
            }

            throw new KeyNotFoundException();
        }
    }
}