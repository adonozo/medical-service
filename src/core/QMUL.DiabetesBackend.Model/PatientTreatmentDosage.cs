using System;
using System.Collections.Generic;

namespace QMUL.DiabetesBackend.Model
{
    public class PatientTreatmentDosage
    {
        public Guid Id { get; set; }
        
        public Guid PatientId { get; set; }
        
        public DateTime Date { get; set; }
        
        public DateTime NextAppointment { get; set; }
        
        public List<MedicationDosage> Medication { get; set; }
    }
}