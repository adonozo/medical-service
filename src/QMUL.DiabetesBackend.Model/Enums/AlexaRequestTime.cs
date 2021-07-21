using System.Runtime.Serialization;

namespace QMUL.DiabetesBackend.Model.Enums
{
    public enum AlexaRequestTime
    {
        [EnumMember(Value = "exactTime")] ExactTime,
        [EnumMember(Value = "allDay")] AllDay,
        [EnumMember(Value = "onEvent")] OnEvent, // Refers to FHIR timing event e.g., BC, ADV, MORN, etc. 
    }
}