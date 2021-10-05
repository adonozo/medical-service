using System.Collections.Generic;
using System.Threading.Tasks;
using Patient = QMUL.DiabetesBackend.Model.Patient;

namespace QMUL.DiabetesBackend.ServiceInterfaces
{
    public interface IPatientService
    {
        public Task<List<Patient>> GetPatientList();

        public Task<Patient> CreatePatient(Patient newPatient);

        public Task<Patient> GetPatient(string idOrEmail);
    }
}