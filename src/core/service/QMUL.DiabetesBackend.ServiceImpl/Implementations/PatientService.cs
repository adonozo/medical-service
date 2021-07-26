using System.Collections.Generic;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using QMUL.DiabetesBackend.DataInterfaces;
using QMUL.DiabetesBackend.ServiceInterfaces;
using Patient = QMUL.DiabetesBackend.Model.Patient;

namespace QMUL.DiabetesBackend.ServiceImpl.Implementations
{
    public class PatientService : IPatientService
    {

        private readonly IPatientDao patientDao;
        private readonly ICarePlanDao carePlanDao;

        public PatientService(IPatientDao patientDao, ICarePlanDao carePlanDao)
        {
            this.patientDao = patientDao;
            this.carePlanDao = carePlanDao;
        }

        public Task<List<Patient>> GetPatientList()
        {
            return this.patientDao.GetPatients();
        }

        public Task<Patient> CreatePatient(Patient newPatient)
        {
            return this.patientDao.CreatePatient(newPatient);
        }

        public Task<Patient> GetPatient(string idOrEmail)
        {
            var result = this.patientDao.GetPatientByIdOrEmail(idOrEmail);
            if (result == null)
            {
                throw new KeyNotFoundException();
            }

            return result;
        }

        public async Task<List<CarePlan>> GetPatientCarePlans(string patientIdOrEmail)
        {
            var patient = await this.patientDao.GetPatientByIdOrEmail(patientIdOrEmail);
            if (patient == null)
            {
                throw new KeyNotFoundException();
            }
            return this.carePlanDao.GetCarePlansFor(patient.Id.ToString());
        }
    }
}