namespace QMUL.DiabetesBackend.Model.Constants;

/// <summary>
/// The URLs used to identify extensions in resources.
/// </summary>
public static class Extensions
{
    public const string PatientEmail = "http://hl7.org/fhir/StructureDefinition/Email";

    public const string PatientAlexaId = "http://diabetes-assistant.com/fhir/StructureDefinition/AlexaId";

    public const string TimingStartDate = "http://diabetes-assistant.com/fhir/StructureDefinition/TimingStartDate";

    public const string PatientTimingPreference =
        "http://diabetes-assistant.com/fhir/StructureDefinition/TimingPreference";

    public const string InsulinFlag = "http://diabetes-assistant.com/fhir/StructureDefinition/InsulinFlag";

    public const string CarePlanReference = "http://diabetes-assistant.com/fhir/StructureDefinition/CarePlanReference";

    public const string NeedsStartDateFlag = "http://diabetes-assistant.com/fhir/StructureDefinition/NeedsStartDateFlag";
}