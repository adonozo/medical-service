namespace QMUL.DiabetesBackend.Service.Utils;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using Model;
using Model.Enums;
using Model.Extensions;
using ResourceReference = Model.ResourceReference;

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
    /// Maps an AlexaRequestType to an EventType when there is an equivalence. Otherwise, throws an exception.
    /// </summary>
    /// <param name="requestType">The <see cref="AlexaRequestType"/> request.</param>
    /// <returns>An equivalent <see cref="EventType"/> for the Alexa request.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if there is no equivalence between the Alexa request
    /// and an Event type</exception>
    public static EventType MapRequestToEventType(AlexaRequestType requestType)
    {
        return requestType switch
        {
            AlexaRequestType.Medication => EventType.MedicationDosage,
            AlexaRequestType.Insulin => EventType.InsulinDosage,
            AlexaRequestType.Glucose => EventType.Measurement,
            _ => throw new ArgumentOutOfRangeException(nameof(requestType), requestType, "Invalid request type")
        };
    }

    public static List<HealthEvent> GenerateEventsFrom(DomainResource request, InternalPatient patient) =>
        request switch
        {
            ServiceRequest serviceRequest => GenerateEventsFrom(serviceRequest, patient),
            MedicationRequest medicationRequest => GenerateEventsFrom(medicationRequest, patient),
            _ => throw new ArgumentException("Request is not a service or medication request", nameof(request))
        };

    /// <summary>
    /// Creates a list of events based on medication timings.
    /// </summary>
    /// <param name="request">The medication request</param>
    /// <param name="patient">The medication request's subject</param>
    /// <returns>A List of events for the medication request</returns>
    public static List<HealthEvent> GenerateEventsFrom(MedicationRequest request, InternalPatient patient)
    {
        var events = new List<HealthEvent>();
        var isInsulin = request.HasInsulinFlag();

        foreach (var dosage in request.DosageInstruction)
        {
            var requestReference = new ResourceReference
            {
                EventType = isInsulin ? EventType.InsulinDosage : EventType.MedicationDosage,
                DomainResourceId = request.Id,
                Text = dosage.Text,
                EventReferenceId = dosage.ElementId,
                StartDate = dosage.GetStartDate()?.UtcDateTime
            };
            var eventsGenerator = new EventsGenerator(patient, dosage.Timing, requestReference);
            events.AddRange(eventsGenerator.GetEvents());
        }

        return events;
    }

    /// <summary>
    /// Creates a list of <see cref="HealthEvent"/> from a <see cref="ServiceRequest"/>. The service request's
    /// occurrence must be an instance of <see cref="Hl7.Fhir.Model.Timing"/> to have consistency between
    /// service and medication requests, and to narrow down timing use cases.
    /// </summary>
    /// <param name="request">The medication request</param>
    /// <param name="patient">The medication request's subject</param>
    /// <returns>A List of events for the medication request</returns>
    public static List<HealthEvent> GenerateEventsFrom(ServiceRequest request, InternalPatient patient)
    {
        var requestReference = new ResourceReference
        {
            EventType = EventType.Measurement,
            DomainResourceId = request.Id,
            EventReferenceId = request.Id,
            Text = request.PatientInstruction,
            StartDate = request.GetStartDate()?.UtcDateTime
        };

        return GenerateEventsFrom(patient, request.Occurrence, requestReference);
    }
    
    public static List<HealthEvent> GenerateEventsFrom(ServiceRequest parentServiceRequest, ServiceRequest containedRequest, InternalPatient patient)
    {
        var requestReference = new ResourceReference
        {
            EventType = EventType.Measurement,
            DomainResourceId = parentServiceRequest.Id,
            EventReferenceId = parentServiceRequest.Id,
            Text = containedRequest.PatientInstruction,
            StartDate = parentServiceRequest.GetStartDate()?.UtcDateTime
        };

        return GenerateEventsFrom(patient, containedRequest.Occurrence, requestReference);
    }

    /// <summary>
    /// Creates a Paginated search <see cref="Bundle"/> result from a Paginated list of <see cref="Resource"/>.
    /// </summary>
    /// <param name="paginatedResult">The paginated result with a <see cref="Resource"/> list.</param>
    /// <returns>The paginated search <see cref="Bundle"/>.</returns>
    public static PaginatedResult<Bundle> ToBundleResult(
        this PaginatedResult<IEnumerable<Resource>> paginatedResult)
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

    public static async Task<T> GetResourceOrThrowAsync<T>(Func<Task<T?>> action, Exception exception) where T : Resource
    {
        var resource = await action.Invoke();
        if (resource is null)
        {
            throw exception;
        }

        return resource;
    }

    private static List<HealthEvent> GenerateEventsFrom(InternalPatient patient, DataType occurrence,
        ResourceReference requestReference)
    {
        if (occurrence is not Timing timing)
        {
            throw new InvalidOperationException("ServiceRequest.Occurrence must be a Timing instance");
        }

        var eventsGenerator = new EventsGenerator(patient, timing, requestReference);
        return eventsGenerator.GetEvents().ToList();
    }
}