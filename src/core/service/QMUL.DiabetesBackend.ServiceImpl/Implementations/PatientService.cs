using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using QMUL.DiabetesBackend.DataInterfaces;
using QMUL.DiabetesBackend.ServiceImpl.Utils;
using QMUL.DiabetesBackend.ServiceInterfaces;
using Patient = QMUL.DiabetesBackend.Model.Patient;

namespace QMUL.DiabetesBackend.ServiceImpl.Implementations
{
    public class PatientService : IPatientService
    {
        private readonly IPatientDao patientDao;
        private readonly IMedicationRequestDao medicationRequestDao;

        public PatientService(IPatientDao patientDao, IMedicationRequestDao medicationRequestDao)
        {
            this.patientDao = patientDao;
            this.medicationRequestDao = medicationRequestDao;
        }

        public async Task<List<Patient>> GetPatientList()
        {
            return await this.patientDao.GetPatients();
        }

        public async Task<Patient> CreatePatient(Patient newPatient)
        {
            return await this.patientDao.CreatePatient(newPatient);
        }

        public async Task<Patient> GetPatient(string idOrEmail)
        {
            var result = await this.patientDao.GetPatientByIdOrEmail(idOrEmail);
            if (result == null)
            {
                throw new KeyNotFoundException();
            }

            return result;
        }

        public async Task<Bundle> GetActiveMedicationRequests(string patientIdOrEmail)
        {
            var patient = await this.patientDao.GetPatientByIdOrEmail(patientIdOrEmail);
            if (patient == null)
            {
                throw new KeyNotFoundException();
            }
            
            var bundle = ResourceUtils.GenerateEmptyBundle();
            var medicationRequests = await this.medicationRequestDao.GetActiveMedicationRequests(patient.Id);
            bundle.Entry = medicationRequests.Select(request => new Bundle.EntryComponent {Resource = request})
                .ToList();
            return bundle;
        }
    }
}
