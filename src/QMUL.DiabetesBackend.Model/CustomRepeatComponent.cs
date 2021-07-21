using System;
using System.Runtime.Serialization;
using Hl7.Fhir.Introspection;
using Hl7.Fhir.Model;
using QMUL.DiabetesBackend.Model.Enums;

namespace QMUL.DiabetesBackend.Model
{
    [DataContract]
    [FhirType("Timing#Repeat", IsNestedType = true)]
    [Serializable]
    public class CustomRepeatComponent : Timing.RepeatComponent
    {
        public Time Time { get; set; }
        
        public new CustomEventTiming?[] When { get; set; }
    }
}