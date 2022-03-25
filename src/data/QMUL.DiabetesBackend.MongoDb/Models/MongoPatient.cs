namespace QMUL.DiabetesBackend.MongoDb.Models
{
    using Hl7.Fhir.Model;

    public class MongoPatient : Patient
    {
        public string Email { get; set; }
    }
}