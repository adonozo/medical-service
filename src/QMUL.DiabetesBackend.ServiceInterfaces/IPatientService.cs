using System.Collections.Generic;
using QMUL.DiabetesBackend.Model;

namespace QMUL.DiabetesBackend.ServiceInterfaces
{
    public interface IPatientService
    {
        public List<Patient> GetPatientList();

        public Patient CreatePatient(Patient newPatient);
    }
}