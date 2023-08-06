using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("QMUL.DiabetesBackend.Service.Tests")]

namespace QMUL.DiabetesBackend.Service.Utils;

using System;
using System.Collections.Generic;
using System.Linq;
using Hl7.Fhir.Model;
using Model;
using Model.Enums;
using Model.Extensions;
using Model.Utils;
using NodaTime;
using NodaTime.Extensions;
using NodaTime.Text;
using Duration = Hl7.Fhir.Model.Duration;
using Period = Hl7.Fhir.Model.Period;

/// <summary>
/// Generates health events based on a Resource frequency/occurrence.
/// </summary>
internal class EventsGenerator<T> where T : Resource
{
    private readonly InternalPatient patient;
    private readonly Timing timing;
    private readonly DateInterval resourcePeriod;
    private readonly Interval? datesFilter;
    private readonly T resource;
    private readonly Dosage dosage;

    /// <summary>
    /// Initializes the events generator
    /// </summary>
    /// <param name="patient">The patient associated with the resource.</param>
    /// <param name="timing">The resource's timing setting. It must have the Bounds field as an instance of <see cref="Hl7.Fhir.Model.Period"/>
    /// or <see cref="Hl7.Fhir.Model.Duration"/>. The only supported period is 1. Also, it must have the TimeOfDay or When field.</param>
    /// <param name="datesFilter">An optional filter to limit events to a start and end dates</param>
    public EventsGenerator(InternalPatient patient,
        Timing timing,
        Interval? datesFilter = null,
        T domainResource = null,
        Dosage dosage = null)
    {
        this.patient = patient;
        this.timing = timing;
        this.datesFilter = datesFilter;
        this.resourcePeriod = this.GetResourceDates();
        this.resource = domainResource;
        this.dosage = dosage;
    }

    public static EventsGenerator<ServiceRequest> ForServiceRequest(InternalPatient patient,
        ServiceRequest serviceRequest,
        Interval? datesFilter = null)
    {
        if (serviceRequest.Occurrence is not Timing timing)
        {
            throw new ArgumentException($"Invalid occurrence for {serviceRequest.Id}", nameof(serviceRequest));
        }

        return new EventsGenerator<ServiceRequest>(patient,
            timing,
            datesFilter,
            serviceRequest);
    }

    public static EventsGenerator<MedicationRequest> ForMedicationRequest(InternalPatient patient,
        Dosage dosage,
        MedicationRequest medicationRequest,
        Interval? datesFilter = null)
    {
        if (!medicationRequest.DosageInstruction.Exists(instruction => instruction.ElementId == dosage.ElementId))
        {
            throw new ArgumentException($"Dosage {dosage.ElementId} does not exists in the Medication request", nameof(dosage));
        }

        return new EventsGenerator<MedicationRequest>(patient,
            dosage.Timing,
            datesFilter,
            medicationRequest,
            dosage);
    }

    /// <summary>
    /// Creates a list of <see cref="HealthEvent{DomainResource}"/> based on a timing instance, a patient and a resource reference.
    /// For instance, a medication request for 14 days - daily would produce 14 health events.
    /// If the timing is 'duration' and resource doesn't have a start date, no event will be generated
    /// </summary>
    /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="HealthEvent{DmainResource}"/> corresponding to the timing.</returns>
    /// <exception cref="InvalidOperationException">If the timing is not properly configured</exception>
    public IEnumerable<HealthEvent<T>> GetEvents()
    {
        if (this.DatesFilterIsOutsideResourceDates())
        {
            return Array.Empty<HealthEvent<T>>();
        }

        if (this.timing.Repeat.DayOfWeek.Any())
        {
            return this.GenerateWeaklyEvents(this.timing.Repeat.DayOfWeek.ToArray());
        }

        // Only a day period is supported; e.g., 3 times a day: frequency = 3, periodUnit = 'd'
        return this.timing.Repeat.Period switch
        {
            1 when this.timing.Repeat.PeriodUnit == Timing.UnitsOfTime.D && this.timing.Repeat.Frequency > 1 =>
                this.GenerateEventsOnMultipleFrequency(),
            1 when this.timing.Repeat.PeriodUnit == Timing.UnitsOfTime.D => this.GenerateDailyEvents(),
            _ => throw new InvalidOperationException("Dosage timing not supported yet. Please review the period.")
        };
    }

    private DateInterval GetResourceDates()
    {
        return this.timing.Repeat.Bounds switch
        {
            Period bounds => bounds.GetDatesFromPeriod(),
            Duration duration => this.GetDurationInterval(duration),
            _ => throw new InvalidOperationException("Dosage or occurrence does not have a valid timing")
        };
    }

    private DateInterval GetDurationInterval(Duration duration)
    {
        if (duration.Value is null or < 0)
        {
            throw new InvalidOperationException(
                $"Duration for {this.resource?.Id} has an invalid value");
        }

        var resourceStartDate = this.timing.GetStartDate();
        if (resourceStartDate is null)
        {
            throw new InvalidOperationException($"Timing in request {this.resource?.Id} does not have a start date");
        }

        var durationValue = (int)duration.Value;
        var durationDays = duration switch
        {
           { Unit: "d" } => durationValue,
           { Unit: "wk" } => durationValue * 7,
           { Unit: "mo" } => (resourceStartDate.Value.PlusMonths(durationValue) - resourceStartDate.Value).Days,
           _ => throw new InvalidOperationException("Dosage or occurrence does not have a valid timing")
        };

        // End date is already inclusive in a date interval, thus -1 day.
        return new DateInterval(resourceStartDate.Value, resourceStartDate.Value.PlusDays(durationDays - 1));
    }

    private IEnumerable<HealthEvent<T>> GenerateDailyEvents()
    {
        var events = new List<HealthEvent<T>>();
        for (var i = 0; i < this.resourcePeriod.Length; i++)
        {
            var day = this.resourcePeriod.Start.PlusDays(i);
            events.AddRange(this.GenerateEventsOnSingleFrequency(day));
        }

        return events;
    }

    private IEnumerable<HealthEvent<T>> GenerateWeaklyEvents(DaysOfWeek?[] daysOfWeek)
    {
        var events = new List<HealthEvent<T>>();
        for (var i = 0; i < this.resourcePeriod.Length; i++)
        {
            var day = this.resourcePeriod.Start.PlusDays(i).DayOfWeek;
            if (daysOfWeek.Any(dayOfWeek => dayOfWeek.ToDayOfWeek().ToIsoDayOfWeek() == day))
            {
                events.AddRange(this.GenerateEventsOnSingleFrequency(this.resourcePeriod.Start.PlusDays(i)));
            }
        }

        return events;
    }

    private IEnumerable<HealthEvent<T>> GenerateEventsOnSingleFrequency(LocalDate date)
    {
        var events = new List<HealthEvent<T>>();
        if (this.timing.Repeat.TimeOfDay.Any())
        {
            foreach (var time in this.timing.Repeat.TimeOfDayIso())
            {
                var localTime = LocalTimePattern.GeneralIso.Parse(time);
                var localDateTime = date.At(localTime.GetValueOrThrow());
                if (this.FilterIncludesDateTime(localDateTime))
                {
                    events.Add(new HealthEvent<T>
                    {
                        ScheduledDateTime = localDateTime,
                        EventTiming = CustomEventTiming.EXACT,
                        Resource = this.resource,
                        Dosage = this.dosage
                    });
                }
            }
        }
        else if (this.timing.Repeat.When.Any())
        {
            foreach (var eventTiming in this.timing.Repeat.When)
            {
                var customTiming = eventTiming.ToCustomEventTiming();
                var timeIsSet = patient.ExactEventTimes.ContainsKey(customTiming);

                var eventDate = timeIsSet
                    ? date.At(patient.ExactEventTimes[customTiming])
                    : throw new InvalidOperationException($"Timing event {eventTiming} for patient {this.patient.Id} does not have a time");

                if (this.FilterIncludesDateTime(eventDate))
                {
                    events.Add(new HealthEvent<T>
                    {
                        ScheduledDateTime = eventDate,
                        EventTiming = customTiming,
                        Resource = this.resource,
                        Dosage = this.dosage
                    });
                }
            }
        }

        return events;
    }

    private IEnumerable<HealthEvent<T>> GenerateEventsOnMultipleFrequency()
    {
        var startDate = this.resourcePeriod.Start;
        var startTime = this.timing.GetStartTime();
        if (startTime is null)
        {
            throw new InvalidOperationException(
                $"Timing in request {this.resource?.Id} does not have a start time");
        }

        var startDateTime = startDate.At(startTime.Value);
        var events = new List<HealthEvent<T>>();
        var totalOccurrences = this.resourcePeriod.Length * this.timing.Repeat.Frequency ?? 0;
        var hourOfDistance = 24 / this.timing.Repeat.Frequency ?? 24;
        for (var i = 0; i < totalOccurrences; i++)
        {
            if (this.FilterIncludesDateTime(startDateTime))
            {
                events.Add(new HealthEvent<T>
                {
                    ScheduledDateTime = startDateTime,
                    EventTiming = CustomEventTiming.EXACT,
                    Resource = this.resource,
                    Dosage = this.dosage
                });
            }

            startDateTime = startDateTime.PlusHours(hourOfDistance);
        }

        return events;
    }

    private bool DatesFilterIsOutsideResourceDates() =>
        this.datesFilter is not null &&
        (this.datesFilter.Value.Start > DateUtils.InstantFromUtcDate(this.resourcePeriod.End) ||
         this.datesFilter.Value.End < DateUtils.InstantFromUtcDate(this.resourcePeriod.Start));

    private bool FilterIncludesDateTime(LocalDateTime dateTime)
    {
        if (!this.datesFilter.HasValue)
        {
            return true;
        }

        var instant = DateUtils.InstantFromUtcDateTime(dateTime);
        return this.datesFilter.Value.Contains(instant);
    }
}