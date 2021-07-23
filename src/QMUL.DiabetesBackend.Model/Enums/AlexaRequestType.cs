using System.Runtime.Serialization;

namespace QMUL.DiabetesBackend.Model.Enums
{
    public enum AlexaRequestType
    {
        [EnumMember(Value = "medication")] Medication,
        [EnumMember(Value = "insulin")] Insulin,
        [EnumMember(Value = "glucose")] Glucose,
        [EnumMember(Value = "appointment")] Appointment,
        [EnumMember(Value = "carePlan")] CarePlan,
    }
}