using System.Collections.Generic;
using QMUL.DiabetesBackend.Model;

namespace QMUL.DiabetesBackend.DataInterfaces
{
    public interface ITreatmentDosage
    {
        public List<PatientTreatmentDosage> GetPatientTreatments();

        public PatientTreatmentDosage GetSinglePatientTreatment();
    }
}