namespace QMUL.DiabetesBackend.Model.Extensions;

using Constants;
using Hl7.Fhir.Model;

public static class ServiceRequestExtensions
{
    /// <summary>
    /// Creates a <see cref="ResourceReference"/> for a <see cref="ServiceRequest"/>. The reference contains the
    /// service request path and the service request ID
    /// </summary>
    /// <param name="serviceRequest">The <see cref="ServiceRequest"/> to create the reference for</param>
    /// <returns>A <see cref="ResourceReference"/></returns>
    public static ResourceReference CreateReference(this ServiceRequest serviceRequest)
    {
        return new ResourceReference
        {
            Type = nameof(ServiceRequest),
            Reference = Constants.ServiceRequestPath + serviceRequest.Id
        };
    }
}