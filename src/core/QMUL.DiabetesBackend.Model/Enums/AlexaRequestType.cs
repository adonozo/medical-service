namespace QMUL.DiabetesBackend.Model.Enums;

using System.Runtime.Serialization;

/// <summary>
/// The information the request asks for: Medication, Insulin, Glucose, Appointment, Careplan
/// </summary>
public enum AlexaRequestType
{
    [EnumMember(Value = "medication")] Medication,
    [EnumMember(Value = "insulin")] Insulin,
    [EnumMember(Value = "glucose")] Glucose,
    [EnumMember(Value = "appointment")] Appointment,
    [EnumMember(Value = "carePlan")] CarePlan,
}