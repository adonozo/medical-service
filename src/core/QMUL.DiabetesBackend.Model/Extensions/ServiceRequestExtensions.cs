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

    /// <summary>
    /// Checks if a service request needs a start date by checking the <see cref="Timing.RepeatComponent"/> property
    /// </summary>
    /// <param name="serviceRequest">The <see cref="ServiceRequest"/></param>
    /// <returns>True if the service request needs a start date</returns>
    public static bool NeedsStartDate(this ServiceRequest serviceRequest)
    {
        return serviceRequest.Occurrence is Timing timing
               && timing.NeedsStartDate() && timing.GetPatientStartDate() is null;
    }

    /// <summary>
    /// Checks if a service request needs a start time by checking the <see cref="Timing.RepeatComponent"/> property
    /// </summary>
    /// <param name="serviceRequest">The <see cref="ServiceRequest"/></param>
    /// <returns>True if the service request needs a start time</returns>
    public static bool NeedsStartTime(this ServiceRequest serviceRequest)
    {
        return serviceRequest.Occurrence is Timing timing
               && timing.NeedsStartTime() && timing.GetPatientStartTime() is null;
    }
}