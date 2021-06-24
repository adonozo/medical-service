using System;
using System.Collections.Generic;
using QMUL.DiabetesBackend.Model;

namespace QMUL.DiabetesBackend.DataInterfaces
{
    public interface ITreatmentDosage
    {
        public List<PatientTreatmentDosage> GetPatientTreatments();

        public PatientTreatmentDosage GetSinglePatientTreatment();

        public PatientTreatmentDosage CreatePatientTreatment(PatientTreatmentDosage newTreatment);

        public PatientTreatmentDosage UpdateTreatment(PatientTreatmentDosage updatedTreatment);

        public bool DeletePatientTreatment(Guid treatmentId);
    }
}