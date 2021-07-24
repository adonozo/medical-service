using System.Collections.Generic;
using System.Threading.Tasks;
using QMUL.DiabetesBackend.Model;

namespace QMUL.DiabetesBackend.DataInterfaces
{
    public interface IPatientDao
    {
        public Task<List<Patient>> GetPatients();

        public Task<Patient> CreatePatient(Patient newPatient);

        public Task<Patient> GetPatientByIdOrEmail(string idOrEmail);
    }
}