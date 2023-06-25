namespace QMUL.DiabetesBackend.Model.Extensions;

using Constants;
using Hl7.Fhir.Model;

public static class ResourceExtensions
{
    /// <summary>
    /// Adds a care plan ID as a <see cref="Extensions.CarePlanReference"/> to a given <see cref="DomainResource"/>
    /// </summary>
    /// <param name="resource">The <see cref="DomainResource"/> to add a reference to</param>
    /// <param name="carePlanId">The care plan ID to add as a reference</param>
    public static void SetCarePlanReference(this DomainResource resource, string carePlanId)
    {
        resource.SetStringExtension(Extensions.CarePlanReference, carePlanId);
    }

    /// <summary>
    /// Gets a care plan reference from a <see cref="DomainResource"/>
    /// </summary>
    /// <param name="resource">The <see cref="DomainResource"/> to get the reference from</param>
    /// <returns>The care plan reference as a string extension, or null if it is not set</returns>
    public static string GetCarePlanReference(this DomainResource resource)
    {
        return resource.GetStringExtension(Extensions.CarePlanReference);
    }
}