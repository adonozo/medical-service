namespace QMUL.DiabetesBackend.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Enums;
    using Hl7.Fhir.Model;

    public static class PatientExtensions
    {
        public static string GetEmailExtension(this Patient patient)
        {
            var email = patient.ModifierExtension
                .Where(mod => mod.Url == "http://hl7.org/fhir/StructureDefinition/Email")
                .Select(z => z.Value)
                .FirstOrDefault() as FhirString;
            return email?.Value;
        }

        public static Dictionary<CustomEventTiming, DateTimeOffset> GetTimingPreference(this Patient patient)
        {
            var startDates = new Dictionary<CustomEventTiming, DateTimeOffset>();
            var preferenceExtension = patient
                .GetExtensions("http://diabetes-assistant.com/fhir/StructureDefinition/TimingPreference")
                .FirstOrDefault();
            if (preferenceExtension == null)
            {
                return startDates;
            }

            var preferences = preferenceExtension.Extension
                .Where(ext => Enum.TryParse<CustomEventTiming>(ext.Url, out _) && ext.Value is FhirDateTime)
                .ToDictionary(ext => Enum.Parse<CustomEventTiming>(ext.Url),
                    ext =>
                    {
                        var date = ext.Value as FhirDateTime;
                        if (date != null && date.TryToDateTimeOffset(out var dateTimeOffset))
                        {
                            return dateTimeOffset;
                        }

                        return DateTimeOffset.UtcNow;
                    });
            return preferences;
        }

        public static void SetTimingPreferences(this Patient patient, Dictionary<CustomEventTiming, DateTimeOffset> preferences)
        {
            patient.ModifierExtension = new List<Extension>();
            var timingExtension = new Extension
            {
                Url = "http://diabetes-assistant.com/fhir/StructureDefinition/TimingPreference"
            };

            foreach (var preference in preferences)
            {
                var uri = preference.Key.ToString();
                var dateTime = new FhirDateTime(preference.Value);
                timingExtension.SetExtension(uri, dateTime);
            }
            patient.ModifierExtension.Add(timingExtension);
        }
        
        public static InternalPatient ToInternalPatient(this Patient patient)
        {
            var alexaId = patient.ModifierExtension
                .Where(mod => mod.Url == "http://diabetes-assistant.com/fhir/StructureDefinition/AlexaId")
                .Select(z => z.Value)
                .FirstOrDefault() as FhirString;
            var internalPatient = new InternalPatient
            {
                Id = patient.Id,
                Email = patient.GetEmailExtension(),
                AlexaUserId = alexaId?.Value,
                ExactEventTimes = patient.GetTimingPreference()
            };

            return internalPatient;
        }
    }
}