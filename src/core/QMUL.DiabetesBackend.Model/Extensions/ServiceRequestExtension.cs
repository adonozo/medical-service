namespace QMUL.DiabetesBackend.Model.Extensions;

using System;
using Constants;
using Hl7.Fhir.Model;

public static class ServiceRequestExtension
{
    public static DateTimeOffset? GetStartDate(this ServiceRequest serviceRequest)
    {
        var extension =
            serviceRequest.GetExtension(Extensions.ServiceRequestStartDate);
        var startDate = extension?.Value as FhirDateTime;
        if (startDate != null && startDate.TryToDateTimeOffset(out var result))
        {
            return result;
        }

        return null;
    }

    /// <summary>
    /// Holds the exact date for a resource to start. Should be used when the resource has a frequency rather than a
    /// period. For example, a medication that must be taken for 14 days.
    /// </summary>
    /// <param name="serviceRequest">The service request</param>
    /// <param name="date">The date when the service request has/will start</param>
    public static void SetStartDate(this ServiceRequest serviceRequest, DateTimeOffset date)
    {
        var fhirDate = new FhirDateTime(date);
        serviceRequest.SetExtension(Extensions.ServiceRequestStartDate, fhirDate);
    }
}