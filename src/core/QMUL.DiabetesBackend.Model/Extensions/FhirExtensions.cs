namespace QMUL.DiabetesBackend.Model.Extensions;

using Constants;
using Hl7.Fhir.Model;

public static class FhirExtensions
{
    /// <summary>
    /// Sets the Reference field in the <see cref="ResourceReference"/> object with a patient reference. The FHIR
    /// convention is to use the path + id, e.g., Patient/{patientId}. This is usually for a patient-related resource,
    /// e.g., a Medication Request.
    /// </summary>
    /// <param name="resource">The <see cref="ResourceReference"/> to populate.</param>
    /// <param name="patientId">The patient ID.</param>
    public static void SetPatientReference(this ResourceReference resource, string patientId)
    {
        resource.Reference = Constants.PatientPath + patientId;
    }

    /// <summary>
    /// Gets an ID from a <see cref="ResourceReference"/> object. It basically removes the path from the
    /// original string. The retrieved ID is not guaranteed to exist.
    /// </summary>
    /// <param name="resource">The <see cref="ResourceReference"/> to extract the ID from.</param>
    /// <returns>An ID or an empty string.</returns>
    public static string GetIdFromReference(this ResourceReference resource)
    {
        return resource.Reference is null ? string.Empty : resource.Reference[(resource.Reference.IndexOf('/') + 1)..];
    }
}