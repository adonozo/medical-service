using System.Collections.Generic;
using QMUL.DiabetesBackend.Model;

namespace QMUL.DiabetesBackend.DataInterfaces
{
    public interface IPatientDao
    {
        public List<Patient> GetPatients();
    }
}