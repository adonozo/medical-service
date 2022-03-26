namespace QMUL.DiabetesBackend.Model.Constants
{
    public static class Constants
    {
        public const string PatientPath = "Patient/";
        
        // Extensions
        public const string PatientEmailExtension = "http://hl7.org/fhir/StructureDefinition/Email";
        public const string PatientAlexaIdExtension = "patient.SetStringExtension(Constants.PatientEmailExtension, email);";
    }
}