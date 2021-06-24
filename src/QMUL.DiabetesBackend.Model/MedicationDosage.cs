using System;

namespace QMUL.DiabetesBackend.Model
{
    public class MedicationDosage
    {
        public Guid Id { get; set; }
        
        public Guid MedicationId { get; set; }
        
        public string Dose { get; set; }
        
        public string DoseUnit { get; set; }
        
        public string Frequency { get; set; }
        
        public string Indications { get; set; }
        
        public bool CreateReminder { get; set; }
    }
}