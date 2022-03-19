namespace QMUL.DiabetesBackend.Model.Enums
{
    using System.Runtime.Serialization;
    using Hl7.Fhir.Utility;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ContactPointUse
    {
        /// <summary>
        /// A communication contact point at a home; attempted contacts for business purposes might intrude privacy and chances are one will contact family or other household members instead of the person one wishes to call. Typically used with urgent cases, or if no other contacts are available.
        /// (system: http://hl7.org/fhir/contact-point-use)
        /// </summary>
        [EnumMember(Value = "home")]
        [EnumLiteral("home", "http://hl7.org/fhir/contact-point-use"), Description("Home")] Home,
        /// <summary>
        /// An office contact point. First choice for business related contacts during business hours.
        /// (system: http://hl7.org/fhir/contact-point-use)
        /// </summary>
        [EnumMember(Value = "work")]
        [EnumLiteral("work", "http://hl7.org/fhir/contact-point-use"), Description("Work")] Work,
        /// <summary>
        /// A temporary contact point. The period can provide more detailed information.
        /// (system: http://hl7.org/fhir/contact-point-use)
        /// </summary>
        [EnumMember(Value = "temp")]
        [EnumLiteral("temp", "http://hl7.org/fhir/contact-point-use"), Description("Temp")] Temp,
        /// <summary>
        /// This contact point is no longer in use (or was never correct, but retained for records).
        /// (system: http://hl7.org/fhir/contact-point-use)
        /// </summary>
        [EnumMember(Value = "old")]
        [EnumLiteral("old", "http://hl7.org/fhir/contact-point-use"), Description("Old")] Old,
        /// <summary>
        /// A telecommunication device that moves and stays with its owner. May have characteristics of all other use codes, suitable for urgent matters, not the first choice for routine business.
        /// (system: http://hl7.org/fhir/contact-point-use)
        /// </summary>
        [EnumMember(Value = "mobile")]
        [EnumLiteral("mobile", "http://hl7.org/fhir/contact-point-use"), Description("Mobile")] Mobile,
    }
}