namespace QMUL.DiabetesBackend.Model.Extensions;

using Constants;
using Hl7.Fhir.Model;

public static class ResourceExtensions
{
    public static void SetCarePlanReference(this DomainResource resource, string carePlanId)
    {
        resource.SetStringExtension(Extensions.CarePlanReference, carePlanId);
    }

    public static string GetCarePlanReference(this DomainResource resource)
    {
        return resource.GetStringExtension(Extensions.CarePlanReference);
    }
}