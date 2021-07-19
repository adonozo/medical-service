using System.Collections.Generic;
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

        public List<Patient> GetPatientList()
        {
            return this.patientDao.GetPatients();
        }

        public Patient CreatePatient(Patient newPatient)
        {
            return this.patientDao.CreatePatient(newPatient);
        }

        public Patient GetPatient(string idOrEmail)
        {
            var result = this.patientDao.GetPatientByIdOrEmail(idOrEmail);
            if (result == null)
            {
                throw new KeyNotFoundException();
            }

            return result;
        }

        public List<CarePlan> GetPatientCarePlans(string patientIdOrEmail)
        {
            var patient = this.patientDao.GetPatientByIdOrEmail(patientIdOrEmail);
            if (patient == null)
            {
                throw new KeyNotFoundException();
            }
            return this.carePlanDao.GetCarePlansFor(patient.Id.ToString());
        }
    }
}