namespace QMUL.DiabetesBackend.Service.Utils;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using Model;
using Model.Exceptions;
using NodaTime;

/// <summary>
/// Resource util methods
/// </summary>
public static class ResourceUtils
{
    /// <summary>
    /// Creates and populates a search Bundle of type <see cref="Bundle.BundleType.Searchset"/>. It also adds the
    /// search timestamp as Utc.Now
    /// </summary>
    /// <param name="resources">The search result resources.</param>
    /// <returns>A <see cref="Bundle"/> search object.</returns>
    public static Bundle GenerateSearchBundle(IEnumerable<Resource> resources)
    {
        var bundle = new Bundle
        {
            Id = Guid.NewGuid().ToString(),
            Type = Bundle.BundleType.Searchset,
            Timestamp = DateTimeOffset.UtcNow,
        };

        foreach (var resource in resources)
        {
            var identity = resource.ResourceIdentity();
            var baseUrl = resource.ResourceBase?.ToString() ?? string.Empty;
            var url = $"{baseUrl}/{identity?.ResourceType}/{identity?.Id}";
            bundle.AddSearchEntry(resource, url, Bundle.SearchEntryMode.Match);
        }

        return bundle;
    }

    /// <summary>
    /// Creates a list of events based on medication timings.
    /// </summary>
    /// <param name="request">The medication request</param>
    /// <param name="patient">The medication request's subject</param>
    /// <param name="dateFilter">An optional end date filter</param>
    /// <returns>A List of events for the medication request</returns>
    public static List<HealthEvent> GenerateEventsFrom(MedicationRequest request,
        InternalPatient patient,
        Interval? dateFilter = null)
    {
        var events = new List<HealthEvent>();

        foreach (var dosage in request.DosageInstruction)
        {
            var eventsGenerator = new EventsGenerator(patient, dosage.Timing, dateFilter);
            events.AddRange(eventsGenerator.GetEvents());
        }

        return events;
    }

    /// <summary>
    /// Gets the list of <see cref="Dosage"/> in a <see cref="MedicationRequest"/> that occurs within the provided
    /// filter. It takes into account the patient's timing preferences
    /// </summary>
    /// <param name="request">The <see cref="MedicationRequest"/> that contains dosages</param>
    /// <param name="patient">The patient for whom the request is for</param>
    /// <param name="dateFilter">The date filter as an <see cref="Interval"/></param>
    /// <returns></returns>
    public static List<Dosage> FilterDosagesOutsideFilter(MedicationRequest request,
        InternalPatient patient,
        Interval dateFilter)
    {
        return request.DosageInstruction
            .Where(dosage => DosageOccursInDate(dosage, patient, dateFilter))
            .ToList();
    }

    /// <summary>
    /// Checks if a <see cref="ServiceRequest"/> occurs in the provided filter, taking into account the patient's
    /// timing preferences
    /// </summary>
    /// <param name="request">The service request</param>
    /// <param name="patient">The patient for whom the service request if for</param>
    /// <param name="dateFilter">The date filter as an <see cref="Interval"/></param>
    /// <returns>True is the service request has an occurrence within the interval</returns>
    /// <exception cref="InvalidOperationException">If the service request's occurrence is not a <see cref="Timing"/> instance</exception>
    public static bool ServiceRequestOccursInDate(ServiceRequest request,
        InternalPatient patient,
        Interval dateFilter)
    {
        var events = new List<HealthEvent>();
        request.Contained.ForEach(resource =>
        {
            if (resource is not ServiceRequest)
            {
                throw new ValidationException("Contained resources are not of type Service Request");
            }

            var eventsGenerator = new EventsGenerator(patient,
                request.Occurrence as Timing ?? throw new InvalidOperationException("Invalid service request occurrence"),
                dateFilter);
            events.AddRange(eventsGenerator.GetEvents());
        });

        return events.Any();
    }

    /// <summary>
    /// Creates a list of <see cref="HealthEvent"/> from a <see cref="ServiceRequest"/>. The service request's
    /// occurrence must be an instance of <see cref="Hl7.Fhir.Model.Timing"/> to have consistency between
    /// service and medication requests, and to narrow down timing use cases.
    /// </summary>
    /// <param name="request">The medication request</param>
    /// <param name="patient">The medication request's subject</param>
    /// <param name="dateFilter">An options date filter</param>
    /// <returns>A List of events for the medication request</returns>
    public static List<HealthEvent> GenerateEventsFrom(ServiceRequest request,
        InternalPatient patient,
        Interval? dateFilter = null)
    {
        var events = new List<HealthEvent>();
        if (request.Occurrence is not Timing)
        {
            throw new InvalidOperationException($"Service request {request.Id} occurrence is not a timing instance");
        }

        request.Contained.ForEach(resource =>
        {
            if (resource is not ServiceRequest)
            {
                throw new ValidationException("Contained resources are not of type Service Request");
            }

            var eventsGenerator = new EventsGenerator(patient,
                request.Occurrence as Timing ?? throw new InvalidOperationException("Invalid service request occurrence"),
                dateFilter);
            events.AddRange(eventsGenerator.GetEvents());
        });

        return events;
    }

    /// <summary>
    /// Creates a Paginated search <see cref="Bundle"/> result from a Paginated list of <see cref="Resource"/>.
    /// </summary>
    /// <param name="paginatedResult">The paginated result with a <see cref="Resource"/> list.</param>
    /// <returns>The paginated search <see cref="Bundle"/>.</returns>
    public static PaginatedResult<Bundle> ToBundleResult<T>(this PaginatedResult<IEnumerable<T>> paginatedResult)
        where T : Resource
    {
        var bundle = GenerateSearchBundle(paginatedResult.Results);
        bundle.Total = (int)paginatedResult.TotalResults;

        return new PaginatedResult<Bundle>
        {
            Results = bundle,
            TotalResults = paginatedResult.TotalResults,
            LastDataCursor = paginatedResult.LastDataCursor,
            RemainingCount = paginatedResult.RemainingCount
        };
    }

    /// <summary>
    /// Tries to get a <see cref="Resource"/>, throws a given exception if the resource is null
    /// </summary>
    /// <param name="action">The async action to get the resource from</param>
    /// <param name="exception">The exception to throw if the resource does not exist</param>
    /// <typeparam name="T">A <see cref="Resource"/> type</typeparam>
    /// <returns>A <see cref="Resource"/></returns>
    /// <exception cref="Exception">If the action does not return a resource</exception>
    public static async Task<T> GetResourceOrThrowAsync<T>(Func<Task<T?>> action, Exception exception) where T : Resource
    {
        var resource = await action.Invoke();
        if (resource is null)
        {
            throw exception;
        }

        return resource;
    }

    private static bool DosageOccursInDate(Dosage dosage,
        InternalPatient patient,
        Interval dateFilter)
    {
        var generator = new EventsGenerator(patient, dosage.Timing, dateFilter);
        var events = generator.GetEvents();
        return events.Any();
    }
}