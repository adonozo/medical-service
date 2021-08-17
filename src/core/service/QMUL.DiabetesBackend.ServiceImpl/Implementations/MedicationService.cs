using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using QMUL.DiabetesBackend.DataInterfaces;
using QMUL.DiabetesBackend.ServiceImpl.Utils;
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

        public async Task<Bundle> GetMedicationList()
        {
            var bundle = ResourceUtils.GenerateEmptyBundle();
            var medications = await this.medicationDao.GetMedicationList();
            bundle.Entry = medications.Select(medication => new Bundle.EntryComponent {Resource = medication})
                .ToList();
            return bundle;
        }

        public async Task<Medication> GetSingleMedication(string id)
        {
            return await ResourceUtils.ValidateObject(
                () => this.medicationDao.GetSingleMedication(id),
                "Unable to find the medication", new KeyNotFoundException());
        }

        public async Task<Medication> CreateMedication(Medication newMedication)
        {
            return await this.medicationDao.CreateMedication(newMedication);
        }
    }
}