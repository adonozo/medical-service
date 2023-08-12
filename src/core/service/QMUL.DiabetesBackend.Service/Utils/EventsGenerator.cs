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
internal class EventsGenerator
{
    private readonly Timing timing;
    private readonly DateInterval resourcePeriod;
    private readonly Interval? datesFilter;

    /// <summary>
    /// Initializes the events generator
    /// </summary>
    /// <param name="timing">The resource's timing setting. It must have the Bounds field as an instance of <see cref="Hl7.Fhir.Model.Period"/>
    /// or <see cref="Hl7.Fhir.Model.Duration"/>. The only supported period is 1. Also, it must have the TimeOfDay or When field.</param>
    /// <param name="datesFilter">An optional filter to limit events to a start and end dates</param>
    public EventsGenerator(Timing timing, Interval? datesFilter = null)
    {
        this.timing = timing;
        this.datesFilter = datesFilter;
        this.resourcePeriod = this.GetResourceDates();
    }

    /// <summary>
    /// Creates a list of <see cref="HealthEvent"/> based on a timing instance, a patient and a resource reference.
    /// For instance, a medication request for 14 days - daily would produce 14 health events.
    /// If the timing is 'duration' and resource doesn't have a start date, no event will be generated
    /// </summary>
    /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="HealthEvent"/> corresponding to the timing.</returns>
    /// <exception cref="InvalidOperationException">If the timing is not properly configured</exception>
    public IEnumerable<HealthEvent> GetEvents()
    {
        if (this.DatesFilterIsOutsideResourceDates())
        {
            return Array.Empty<HealthEvent>();
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
                $"Duration for has an invalid value: {duration.Value}");
        }

        var resourceStartDate = this.timing.GetStartDate();
        if (resourceStartDate is null)
        {
            throw new InvalidOperationException($"Timing in request does not have a start date");
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

    private IEnumerable<HealthEvent> GenerateDailyEvents()
    {
        var events = new List<HealthEvent>();
        for (var i = 0; i < this.resourcePeriod.Length; i++)
        {
            var day = this.resourcePeriod.Start.PlusDays(i);
            events.AddRange(this.GenerateEventsOnSingleFrequency(day));
        }

        return events;
    }

    private IEnumerable<HealthEvent> GenerateWeaklyEvents(DaysOfWeek?[] daysOfWeek)
    {
        var events = new List<HealthEvent>();
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

    private IEnumerable<HealthEvent> GenerateEventsOnSingleFrequency(LocalDate date)
    {
        var events = new List<HealthEvent>();
        if (this.timing.Repeat.TimeOfDay.Any())
        {
            events.AddRange(this.EventsFromTimeOfDay(date));
        }
        else if (this.timing.Repeat.When.Any())
        {
            events.AddRange(this.EventsFromWhen(date));
        }

        return events;
    }

    private IEnumerable<HealthEvent> GenerateEventsOnMultipleFrequency()
    {
        var startDate = this.resourcePeriod.Start;
        var startTime = this.timing.GetStartTime();
        if (startTime is null)
        {
            throw new InvalidOperationException(
                $"Timing does not have a start time");
        }

        var startDateTime = startDate.At(startTime.Value);
        var events = new List<HealthEvent>();
        var totalOccurrences = this.resourcePeriod.Length * this.timing.Repeat.Frequency ?? 0;
        var hourOfDistance = 24 / this.timing.Repeat.Frequency ?? 24;
        for (var i = 0; i < totalOccurrences; i++)
        {
            if (this.FilterIncludesDateTime(startDateTime))
            {
                events.Add(CreateEventAt(startDateTime, CustomEventTiming.EXACT));
            }

            startDateTime = startDateTime.PlusHours(hourOfDistance);
        }

        return events;
    }

    private bool DatesFilterIsOutsideResourceDates() =>
        this.datesFilter is not null &&
        (this.datesFilter.Value.Start > DateUtils.InstantFromUtcDate(this.resourcePeriod.End) ||
         this.datesFilter.Value.End < DateUtils.InstantFromUtcDate(this.resourcePeriod.Start));

    private IEnumerable<HealthEvent> EventsFromTimeOfDay(LocalDate date) => this.timing.Repeat.TimeOfDayIso()
        .Select(time =>
        {
            var localTime = LocalTimePattern.GeneralIso.Parse(time);
            var localDateTime = date.At(localTime.GetValueOrThrow());
            return this.FilterIncludesDateTime(localDateTime)
                ? CreateEventAt(localDateTime, CustomEventTiming.EXACT)
                : null;
        })
        .OfType<HealthEvent>();

    private IEnumerable<HealthEvent> EventsFromWhen(LocalDate date) => this.timing.Repeat.When
        .Select(when =>
        {
            var customTiming = when.ToCustomEventTiming();
            var eventDate = date.AtMidnight();

            return this.FilterIncludesDateTime(eventDate)
                ? CreateEventAt(eventDate, customTiming)
                : null;
        })
        .OfType<HealthEvent>();

    private bool FilterIncludesDateTime(LocalDateTime dateTime)
    {
        if (!this.datesFilter.HasValue)
        {
            return true;
        }

        var instant = DateUtils.InstantFromUtcDateTime(dateTime);
        return this.datesFilter.Value.Contains(instant);
    }

    private static HealthEvent CreateEventAt(LocalDateTime scheduleDateTime, CustomEventTiming customTiming) =>
        new()
        {
            ScheduledDateTime = scheduleDateTime,
            EventTiming = customTiming
        };
}