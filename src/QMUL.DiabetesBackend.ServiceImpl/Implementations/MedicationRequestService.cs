using System.Collections.Generic;
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

        public MedicationRequest CreateMedicationRequest(MedicationRequest request)
        {
            return this.medicationRequestDao.CreateMedicationRequest(request);
        }

        public MedicationRequest GetMedicationRequest(string id)
        {
            return this.medicationRequestDao.GetMedicationRequest(id);
        }

        public MedicationRequest UpdateMedicationRequest(string id, MedicationRequest request)
        {
            var exists = this.medicationRequestDao.GetMedicationRequest(id) != null;
            if (exists)
            {
                return this.medicationRequestDao.UpdateMedicationRequest(id, request);
            }

            throw new KeyNotFoundException();
        }

        public bool DeleteMedicationRequest(string id)
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