namespace QMUL.DiabetesBackend.Model.Extensions;

using Constants;
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
            AlexaUserId = alexaId
        };

        return internalPatient;
    }
}