using System;
using System.Collections.Generic;
using System.Linq;
using QMUL.DiabetesBackend.DataInterfaces;
using QMUL.DiabetesBackend.Model;

namespace QMUL.DiabetesBackend.DataMemory
{
    public class TreatmentMemory : ITreatmentDosageDao
    {
        private readonly List<PatientTreatmentDosage> treatments;

        public TreatmentMemory()
        {
            this.treatments = new List<PatientTreatmentDosage>();
        }

        public List<PatientTreatmentDosage> GetPatientTreatments(Guid patientId)
        {
            return this.treatments.FindAll(treatment => treatment.Id.Equals(patientId));
        }

        public PatientTreatmentDosage GetSinglePatientTreatment(Guid treatmentId)
        {
            return this.treatments.FirstOrDefault(treatment => treatment.Id.Equals(treatmentId));
        }

        public PatientTreatmentDosage CreatePatientTreatment(PatientTreatmentDosage newTreatment)
        {
            newTreatment.Id = Guid.NewGuid();
            this.treatments.Add(newTreatment);
            return newTreatment;
        }

        public PatientTreatmentDosage UpdateTreatment(PatientTreatmentDosage updatedTreatment)
        {
            var currentTreatment = this.GetSinglePatientTreatment(updatedTreatment.Id);
            if (currentTreatment == null)
            {
                return null;
            }
            
            currentTreatment = updatedTreatment;
            return currentTreatment;
        }

        public bool DeletePatientTreatment(Guid treatmentId)
        {
            var previousTreatment = this.GetSinglePatientTreatment(treatmentId);
            if (previousTreatment == null)
            {
                return false;
            }

            this.treatments.RemoveAll(treatment => treatment.Id.Equals(treatmentId));
            return true;
        }
    }
}