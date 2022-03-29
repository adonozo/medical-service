namespace QMUL.DiabetesBackend.Model.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Constants;
    using Enums;
    using Hl7.Fhir.Model;

    public static class PatientExtensions
    {
        public static string GetEmailExtension(this Patient patient)
        {
            return patient.GetStringExtension(Extensions.PatientEmail);
        }

        public static void SetEmailExtension(this Patient patient, string email)
        {
            patient.SetStringExtension(Extensions.PatientEmail, email);
        }

        public static Dictionary<CustomEventTiming, DateTimeOffset> GetTimingPreference(this Patient patient)
        {
            var startDates = new Dictionary<CustomEventTiming, DateTimeOffset>();
            var preferenceExtension = patient
                .GetExtension(Extensions.PatientTimingPreference);
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

        public static void SetTimingPreferences(this Patient patient,
            Dictionary<CustomEventTiming, DateTimeOffset> preferences)
        {
            patient.ModifierExtension = new List<Extension>();
            var timingExtension = new Extension
            {
                Url = Extensions.PatientTimingPreference
            };

            foreach (var preference in preferences)
            {
                var uri = preference.Key.ToString();
                var dateTime = new FhirDateTime(preference.Value);
                timingExtension.SetExtension(uri, dateTime);
            }

            patient.ModifierExtension.Add(timingExtension);
        }

        public static void SetAlexaIdExtension(this Patient patient, string alexaId)
        {
            patient.SetStringExtension(Extensions.PatientAlexaId, alexaId);
        }

        public static InternalPatient ToInternalPatient(this Patient patient)
        {
            var alexaId = patient.GetStringExtension(Extensions.PatientAlexaId);
            var internalPatient = new InternalPatient
            {
                Id = patient.Id,
                Email = patient.GetEmailExtension(),
                AlexaUserId = alexaId,
                ExactEventTimes = patient.GetTimingPreference()
            };

            return internalPatient;
        }
    }
}