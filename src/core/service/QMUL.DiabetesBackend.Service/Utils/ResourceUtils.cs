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
    /// Gets the list of <see cref="Dosage"/> in a <see cref="MedicationRequest"/> that occurs within the provided
    /// filter. It takes into account the patient's timing preferences
    /// </summary>
    /// <param name="request">The <see cref="MedicationRequest"/> that contains dosages</param>
    /// <param name="dateFilter">The date filter as an <see cref="Interval"/></param>
    /// <returns></returns>
    public static List<Dosage> FilterDosagesOutsideFilter(MedicationRequest request,
        Interval dateFilter)
    {
        return request.DosageInstruction
            .Where(dosage => DosageOccursInDate(dosage, dateFilter))
            .ToList();
    }

    /// <summary>
    /// Checks if a <see cref="ServiceRequest"/> occurs in the provided filter, taking into account the patient's
    /// timing preferences
    /// </summary>
    /// <param name="request">The service request</param>
    /// <param name="dateFilter">The date filter as an <see cref="Interval"/></param>
    /// <returns>True is the service request has an occurrence within the interval</returns>
    /// <exception cref="InvalidOperationException">If the service request's occurrence is not a <see cref="Timing"/> instance</exception>
    public static bool ServiceRequestOccursInDate(ServiceRequest request, Interval dateFilter)
    {
        var events = new List<HealthEvent>();
        request.Contained.ForEach(resource =>
        {
            if (resource is not ServiceRequest)
            {
                throw new ValidationException("Contained resources are not of type Service Request");
            }

            var eventsGenerator = new EventsGenerator(
                request.Occurrence as Timing ?? throw new InvalidOperationException("Invalid service request occurrence"),
                dateFilter);
            events.AddRange(eventsGenerator.GetEvents());
        });

        return events.Any();
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
        Interval dateFilter)
    {
        var generator = new EventsGenerator(dosage.Timing, dateFilter);
        var events = generator.GetEvents();
        return events.Any();
    }
}