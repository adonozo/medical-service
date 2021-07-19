using System.Collections.Generic;
using QMUL.DiabetesBackend.Model;

namespace QMUL.DiabetesBackend.DataInterfaces
{
    public interface IPatientDao
    {
        public List<Patient> GetPatients();

        public Patient CreatePatient(Patient newPatient);

        public Patient GetPatientByIdOrEmail(string idOrEmail);
    }
}