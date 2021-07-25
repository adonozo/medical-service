using System.Runtime.Serialization;

namespace QMUL.DiabetesBackend.Model.Enums
{
    /// <summary>
    /// The kind of time period requested: All day, On event, Exact time
    /// </summary>
    public enum AlexaRequestTime
    {
        [EnumMember(Value = "exactTime")] ExactTime,
        [EnumMember(Value = "allDay")] AllDay,
        [EnumMember(Value = "onEvent")] OnEvent, // Refers to FHIR timing event e.g., BC, ADV, MORN, etc. 
    }
}