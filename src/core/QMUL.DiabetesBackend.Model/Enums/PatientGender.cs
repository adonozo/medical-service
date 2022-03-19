namespace QMUL.DiabetesBackend.Model.Enums
{
    using System.Runtime.Serialization;
    using Hl7.Fhir.Utility;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    [JsonConverter(typeof(StringEnumConverter))]
    public enum PatientGender
    {
        /// <summary>
        /// Male.
        /// (system: http://hl7.org/fhir/administrative-gender)
        /// </summary>
        [EnumMember(Value = "male")]
        [EnumLiteral("male", "http://hl7.org/fhir/administrative-gender"), Description("Male")] Male,
        /// <summary>
        /// Female.
        /// (system: http://hl7.org/fhir/administrative-gender)
        /// </summary>
        [EnumMember(Value = "female")]
        [EnumLiteral("female", "http://hl7.org/fhir/administrative-gender"), Description("Female")] Female,
        /// <summary>
        /// Other.
        /// (system: http://hl7.org/fhir/administrative-gender)
        /// </summary>
        [EnumMember(Value = "other")]
        [EnumLiteral("other", "http://hl7.org/fhir/administrative-gender"), Description("Other")] Other,
        /// <summary>
        /// Unknown.
        /// (system: http://hl7.org/fhir/administrative-gender)
        /// </summary>
        [EnumMember(Value = "unknown")]
        [EnumLiteral("unknown", "http://hl7.org/fhir/administrative-gender"), Description("Unknown")] Unknown,
    }
}