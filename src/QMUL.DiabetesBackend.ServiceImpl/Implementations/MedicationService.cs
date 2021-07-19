using System.Collections.Generic;
using Hl7.Fhir.Model;
using QMUL.DiabetesBackend.DataInterfaces;
using QMUL.DiabetesBackend.ServiceInterfaces;

namespace QMUL.DiabetesBackend.ServiceImpl.Implementations
{
    public class MedicationService : IMedicationService
    {
        private readonly IMedicationDao medicationDao;

        public MedicationService(IMedicationDao medicationDao)
        {
            this.medicationDao = medicationDao;
        }

        public List<Medication> GetMedicationList()
        {
            return this.medicationDao.GetMedicationList();
        }
    }
}