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

        private readonly List<PatientTreatmentDosage> sampleTreatments = new()
        {
            new PatientTreatmentDosage
            {
                Id = Guid.Parse("3ca91535-8f78-4bc8-b8ca-f95b21d23c8c"),
                PatientId = Guid.Parse("fb85c38d-5ea5-4263-ba00-3b9528d4c4b3"),
                Date = DateTime.Now,
                NextAppointment = DateTime.Now.AddDays(14),
                Medication = new List<MedicationDosage>
                {
                    new()
                    {
                        Id = Guid.Parse("abba836c-2103-44e9-ac6b-823d886a5ddf"),
                        MedicationId = Guid.Parse("92be6243-8e85-46b1-aa96-c9948ebbed99"),
                        Dose = "5",
                        DoseUnit = "grams",
                        Frequency = "FREQ=DAILY;INTERVAL=1;COUNT=14",
                        Indications = "Inject one dose at least one hour before a meal.",
                        CreateReminder = true
                    }
                }
            }
        };

        public TreatmentMemory()
        {
            this.treatments = sampleTreatments;
        }

        public List<PatientTreatmentDosage> GetPatientTreatments(Guid patientId)
        {
            return this.treatments.FindAll(treatment => treatment.PatientId.Equals(patientId));
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