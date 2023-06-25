namespace QMUL.DiabetesBackend.Model.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;
using Constants;
using Enums;
using Hl7.Fhir.Model;

public static class PatientExtensions
{
    /// <summary>
    /// Gets the email extension from a <see cref="Patient"/> as a string extension 
    /// </summary>
    /// <param name="patient">The <see cref="Patient"/> to get the email extension from</param>
    /// <returns>The patient's email extension, or null if the extension is not set</returns>
    public static string GetEmailExtension(this Patient patient)
    {
        return patient.GetStringExtension(Extensions.PatientEmail);
    }

    /// <summary>
    /// Sets an email extension into a <see cref="Patient"/> as a string extension
    /// </summary>
    /// <param name="patient">The <see cref="Patient"/> to add the extension to</param>
    /// <param name="email">The email to add as an extension</param>
    public static void SetEmailExtension(this Patient patient, string email)
    {
        patient.SetStringExtension(Extensions.PatientEmail, email);
    }

    /// <summary>
    /// Gets a patient's timing preferences from an extension set in the <see cref="Patient"/> object
    /// </summary>
    /// <param name="patient">The <see cref="Patient"/> to get the preferences from</param>
    /// <returns>A dictionary with <see cref="CustomEventTiming"/> as keys and <see cref="DateTimeOffset"/> as values.
    /// Returns an empty dictionary if the preferences are not set</returns>
    public static Dictionary<CustomEventTiming, DateTimeOffset> GetTimingPreference(this Patient patient)
    {
        var preferenceExtension = patient.GetExtension(Extensions.PatientTimingPreference);
        if (preferenceExtension is null)
        {
            return new Dictionary<CustomEventTiming, DateTimeOffset>();
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

    /// <summary>
    /// Sets a patient's timing preferences into a <see cref="Patient"/>. The method will replace any existing preference
    /// </summary>
    /// <param name="patient">The <see cref="Patient"/> to set the timings preferences to</param>
    /// <param name="preferences">A dictionary with <see cref="CustomEventTiming"/> as keys and
    /// <see cref="DateTimeOffset"/> as values</param>
    public static void SetTimingPreferences(this Patient patient,
        Dictionary<CustomEventTiming, DateTimeOffset> preferences)
    {
        var patientTimings = patient.GetExtension(Extensions.PatientTimingPreference);
        var timingExtension = patientTimings
                              ?? new Extension
                              {
                                  Url = Extensions.PatientTimingPreference
                              };

        foreach (var (timing, dateTimeOffset) in preferences)
        {
            var uri = timing.ToString();
            var dateTime = new FhirDateTime(dateTimeOffset);
            timingExtension.SetExtension(uri, dateTime);
        }

        patient.RemoveExtension(Extensions.PatientTimingPreference);
        patient.Extension.Add(timingExtension);
    }

    /// <summary>
    /// Sets the patient's Alexa ID as an string extension
    /// </summary>
    /// <param name="patient">The <see cref="Patient"/> to add the extension to</param>
    /// <param name="alexaId">The Alexa ID</param>
    public static void SetAlexaIdExtension(this Patient patient, string alexaId)
    {
        patient.SetStringExtension(Extensions.PatientAlexaId, alexaId);
    }

    /// <summary>
    /// Maps a <see cref="Patient"/> into an <see cref="InternalPatient"/>
    /// </summary>
    /// <param name="patient">The <see cref="Patient"/> to map</param>
    /// <returns>A <see cref="InternalPatient"/> object</returns>
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