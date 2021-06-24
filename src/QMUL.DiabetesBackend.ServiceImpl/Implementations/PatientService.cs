using System.Collections.Generic;
using QMUL.DiabetesBackend.DataInterfaces;
using QMUL.DiabetesBackend.Model;
using QMUL.DiabetesBackend.ServiceInterfaces;

namespace QMUL.DiabetesBackend.ServiceImpl.Implementations
{
    public class PatientService : IPatientService
    {

        private readonly IPatientDao patientDao;

        public PatientService(IPatientDao patientDao)
        {
            this.patientDao = patientDao;
        }

        public List<Patient> GetPatientList()
        {
            return this.patientDao.GetPatients();
        }

        public Patient CreatePatient(Patient newPatient)
        {
            return this.patientDao.CreatePatient(newPatient);
        }
    }
}