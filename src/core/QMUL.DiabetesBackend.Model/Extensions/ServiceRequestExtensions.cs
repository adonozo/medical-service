namespace QMUL.DiabetesBackend.Model.Extensions;

using System.Linq;
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
    /// <param name="accumulator">The accumulator value used for recursive calls</param>
    /// <returns>True if the service request needs a start time</returns>
    public static bool NeedsStartTime(this ServiceRequest serviceRequest, bool accumulator = false)
    {
        if (accumulator || !serviceRequest.Contained.Any())
        {
            return serviceRequest.Occurrence is Timing timing && timing.NeedsStartTime();
        }

        return serviceRequest.Contained
            .Where(contained => contained is ServiceRequest)
            .Cast<ServiceRequest>()
            .Aggregate(false, (current, request) => request.NeedsStartTime(current));
    }
}