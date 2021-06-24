using System;
using System.Collections.Generic;
using QMUL.DiabetesBackend.Model;

namespace QMUL.DiabetesBackend.ServiceInterfaces
{
    public interface ITreatmentService
    {
        public PatientTreatmentDosage CreateTreatment(PatientTreatmentDosage newTreatment);

        public PatientTreatmentDosage GetSinglePatientTreatment(Guid treatmentId);

        public List<PatientTreatmentDosage> GetPatientTreatments(Guid patientId);

        public PatientTreatmentDosage UpdatePatientTreatment(Guid treatmentId, PatientTreatmentDosage treatment);
        
        public bool DeletePatientTreatment(Guid treatmentId);
    }
}