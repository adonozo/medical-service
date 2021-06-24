using System;
using System.Collections.Generic;
using QMUL.DiabetesBackend.Model;

namespace QMUL.DiabetesBackend.Service
{
    public interface ITreatmentService
    {
        public PatientTreatmentDosage CreateTreatment(PatientTreatmentDosage newTreatment);

        public PatientTreatmentDosage GetSinglePatientTreatment(PatientTreatmentDosage treatment);

        public List<PatientTreatmentDosage> GetPatientTreatments(Guid patientId);

        public PatientTreatmentDosage UpdatePatientTreatment(Guid treatmentId, PatientTreatmentDosage treatment);
        
        public bool DeletePatientTreatment(Guid treatmentId);
    }
}