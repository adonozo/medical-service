namespace QMUL.DiabetesBackend.Model.Extensions;

using Constants;
using Hl7.Fhir.Model;

/// <summary>
/// Extension methods for <see cref="MedicationRequest"/>.
/// </summary>
public static class MedicationRequestExtensions
{
    /// <summary>
    /// Checks if the <see cref="MedicationRequest"/> has the insulin flag extension. This means the medication request
    /// has an insulin type <see cref="Medication"/>.
    /// </summary>
    /// <param name="medicationRequest">The <see cref="MedicationRequest"/>.</param>
    /// <returns>True if the medication request has the insulin flag extension and its value is true. False otherwise</returns>
    public static bool HasInsulinFlag(this MedicationRequest medicationRequest)
    {
        return medicationRequest.GetBoolExtension(Extensions.InsulinFlag) ?? false;
    }

    /// <summary>
    /// Sets an insulin flag to a <see cref="MedicationRequest"/> as a bool extension
    /// </summary>
    /// <param name="medicationRequest">The <see cref="MedicationRequest"/> to set the insulin extension to</param>
    public static void SetInsulinFlag(this MedicationRequest medicationRequest)
    {
        medicationRequest.SetBoolExtension(Extensions.InsulinFlag, true);
    }

    /// <summary>
    /// Creates a <see cref="ResourceReference"/> from a <see cref="MedicationRequest"/>. It will contain the medication
    /// request ID
    /// </summary>
    /// <param name="medicationRequest">The <see cref="MedicationRequest"/> object to create the reference from</param>
    /// <returns>A <see cref="ResourceReference"/> for a medication request</returns>
    public static ResourceReference CreateReference(this MedicationRequest medicationRequest)
    {
        return new ResourceReference
        {
            Type = nameof(MedicationRequest),
            Reference = Constants.MedicationRequestPath + medicationRequest.Id
        };
    }
}