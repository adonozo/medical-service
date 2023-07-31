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
using NodaTime;
using NodaTime.Extensions;
using NodaTime.Text;
using Duration = Hl7.Fhir.Model.Duration;
using Period = Hl7.Fhir.Model.Period;
using ResourceReference = Model.ResourceReference;

/// <summary>
/// Generates health events based on a Resource frequency/occurrence.
/// </summary>
internal class EventsGenerator
{
    private readonly InternalPatient patient;
    private readonly Timing timing;
    private readonly ResourceReference resourceReference;

    /// <summary>
    /// The class constructor
    /// </summary>
    /// <param name="patient">The patient associated with the resource.</param>
    /// <param name="timing">The resource's timing setting. It must have the Bounds field as an instance of <see cref="Hl7.Fhir.Model.Period"/>
    /// or <see cref="Hl7.Fhir.Model.Duration"/>. The only supported period is 1. Also, it must have the TimeOfDay or When field.</param>
    /// <param name="resourceReference">A reference to the source resource, i.e., Medication or Service request.</param>
    public EventsGenerator(InternalPatient patient, Timing timing, ResourceReference resourceReference)
    {
        this.patient = patient;
        this.timing = timing;
        this.resourceReference = resourceReference;
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
        int durationInDays;
        LocalDate startDate;

        switch (this.timing.Repeat.Bounds)
        {
            case Period bounds:
                (startDate, var endDate) = bounds.GetDatesFromPeriod();
                durationInDays = NodaTime.Period.DaysBetween(startDate, endDate) + 1; // Period is end-date inclusive, hence, +1 day.
                break;
            case Duration duration:
                (durationInDays, startDate) = this.GetDurationDaysAndStartDate(duration);
                break;
            default:
                throw new InvalidOperationException("Dosage or occurrence does not have a valid timing");
        }

        if (this.timing.Repeat.DayOfWeek.Any())
        {
            return this.GenerateWeaklyEvents(durationInDays, startDate, this.timing.Repeat.DayOfWeek.ToArray());
        }

        // For now, only a period of 1 is supported; e.g., 3 times a day: frequency = 3, period = 1
        return this.timing.Repeat.Period switch
        {
            1 when this.timing.Repeat.PeriodUnit == Timing.UnitsOfTime.D && this.timing.Repeat.Frequency > 1 =>
                this.GenerateEventsOnMultipleFrequency(durationInDays, startDate),
            1 when this.timing.Repeat.PeriodUnit == Timing.UnitsOfTime.D => this.GenerateDailyEvents(durationInDays, startDate),
            _ => throw new InvalidOperationException("Dosage timing not supported yet. Please review the period.")
        };
    }

    private (int, LocalDate) GetDurationDaysAndStartDate(Duration duration)
    {
        if (duration.Value is null or < 0)
        {
            throw new InvalidOperationException(
                $"Duration for {this.resourceReference.EventReferenceId} has an invalid value");
        }

        var startDate = this.timing.GetStartDate();
        if (startDate is null)
        {
            throw new InvalidOperationException($"Timing in request {this.resourceReference.DomainResourceId} does not have a start date");
        }

        var durationValue = (int)duration.Value;
        var durationDays = duration switch
        {
           { Unit: "d" } => durationValue,
           { Unit: "wk" } => (startDate.Value.PlusWeeks(durationValue) - startDate.Value).Days,
           { Unit: "mo" } => (startDate.Value.PlusMonths(durationValue) - startDate.Value).Days,
           _ => throw new InvalidOperationException("Dosage or occurrence does not have a valid timing")
        };

        return (durationDays, startDate.Value);
    }

    private IEnumerable<HealthEvent> GenerateDailyEvents(int days, LocalDate startDate)
    {
        var events = new List<HealthEvent>();
        for (var i = 0; i < days; i++)
        {
            var day = startDate.PlusDays(i);
            events.AddRange(this.GenerateEventsOnSingleFrequency(day));
        }

        return events;
    }

    private IEnumerable<HealthEvent> GenerateWeaklyEvents(int durationInDays, LocalDate startDate,
        DaysOfWeek?[] daysOfWeek)
    {
        var events = new List<HealthEvent>();
        for (var i = 0; i < durationInDays; i++)
        {
            var day = startDate.PlusDays(i).DayOfWeek;
            if (daysOfWeek.Any(dayOfWeek => dayOfWeek.ToDayOfWeek().ToIsoDayOfWeek() == day))
            {
                events.AddRange(this.GenerateEventsOnSingleFrequency(startDate.PlusDays(i)));
            }
        }

        return events;
    }

    private IEnumerable<HealthEvent> GenerateEventsOnSingleFrequency(LocalDate date)
    {
        var events = new List<HealthEvent>();
        if (this.timing.Repeat.TimeOfDay.Any())
        {
            foreach (var time in this.timing.Repeat.TimeOfDayIso())
            {
                var localTime = LocalTimePattern.GeneralIso.Parse(time);
                var healthEvent = new HealthEvent
                {
                    PatientId = this.patient.Id,
                    ScheduledDateTime = date.At(localTime.GetValueOrThrow()),
                    ExactTimeIsSetup = true,
                    EventTiming = CustomEventTiming.EXACT,
                    ResourceReference = this.resourceReference
                };
                events.Add(healthEvent);
            }
        }
        else if (this.timing.Repeat.When.Any())
        {
            foreach (var eventTiming in this.timing.Repeat.When)
            {
                var customTiming = eventTiming.ToCustomEventTiming();
                var exactTimeIsSetup = patient.ExactEventTimes.ContainsKey(customTiming);

                var eventDate = exactTimeIsSetup
                    ? date.At(patient.ExactEventTimes[customTiming])
                    : throw new InvalidOperationException($"Timing event {eventTiming} for patient {this.patient.Id} does not have a time");
                var healthEvent = new HealthEvent
                {
                    PatientId = this.patient.Id,
                    ScheduledDateTime = eventDate,
                    ExactTimeIsSetup = exactTimeIsSetup,
                    EventTiming = customTiming,
                    ResourceReference = this.resourceReference
                };
                events.Add(healthEvent);
            }
        }

        return events;
    }

    private IEnumerable<HealthEvent> GenerateEventsOnMultipleFrequency(int days, LocalDate startDate)
    {
        var events = new List<HealthEvent>();
        var totalOccurrences = days * this.timing.Repeat.Frequency ?? 0;
        var hourOfDistance = 24 / this.timing.Repeat.Frequency ?? 24;
        for (var i = 0; i < totalOccurrences; i++)
        {
            var date = startDate.At(new LocalTime(hourOfDistance * i, 00)); // todo this won't work, startDate should be LocalDateTime
            var healthEvent = new HealthEvent
            {
                PatientId = this.patient.Id,
                ScheduledDateTime = date,
                ExactTimeIsSetup = true,
                EventTiming = CustomEventTiming.EXACT,
                ResourceReference = this.resourceReference
            };
            events.Add(healthEvent);
        }

        return events;
    }
}