using System.Runtime.CompilerServices;

[assembly:InternalsVisibleTo("QMUL.DiabetesBackend.Service.Tests")]
namespace QMUL.DiabetesBackend.Service.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Hl7.Fhir.Model;
    using Model;
    using Model.Enums;
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
        /// <param name="timing">The resource's timing setting. It must have the Bounds field as an instance of <see cref="Period"/>
        /// or <see cref="Duration"/>. The only supported period is 1. Also, it must have the TimeOfDay or When field.</param>
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
        /// </summary>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="HealthEvent"/> corresponding to the timing.</returns>
        /// <exception cref="InvalidOperationException">If the timing is not properly configured</exception>
        public IEnumerable<HealthEvent> GetEvents()
        {
            int days;
            DateTime startDate;
            switch (timing.Repeat.Bounds)
            {
                case Period bounds:
                    startDate = DateTime.Parse(bounds.Start);
                    var endDate = DateTime.Parse(bounds.End);
                    days = (endDate - startDate).Days + 1; // Period is end-date inclusive, thus, +1 day.
                    break;
                case Duration {Unit: "d"} duration:
                    days = duration.Value == null
                        ? throw new InvalidOperationException("Duration is not defined")
                        : (int) duration.Value;
                    startDate = this.resourceReference.StartDate?.UtcDateTime ?? DateTime.UtcNow;
                    break;
                default:
                    throw new InvalidOperationException("Dosage or occurrence does not have a valid timing");
            }

            if (timing.Repeat.DayOfWeek.Any())
            {
                return this.GenerateWeaklyEvents(days, startDate, timing.Repeat.DayOfWeek);
            }

            // For now, only a period of 1 is supported; e.g., 3 times a day: frequency = 3, period = 1
            return timing.Repeat.Period switch
            {
                1 when timing.Repeat.PeriodUnit == Timing.UnitsOfTime.D && timing.Repeat.Frequency > 1 => this
                    .GenerateEventsOnMultipleFrequency(days, startDate),
                1 when timing.Repeat.PeriodUnit == Timing.UnitsOfTime.D => this.GenerateDailyEvents(days, startDate),
                _ => throw new InvalidOperationException("Dosage timing not supported yet. Please review the period.")
            };
        }

        private IEnumerable<HealthEvent> GenerateDailyEvents(int days, DateTime startDate)
        {
            var events = new List<HealthEvent>();
            for (var i = 0; i < days; i++)
            {
                var day = startDate.AddDays(i);
                events.AddRange(this.GenerateEventsOnSingleFrequency(day));
            }

            return events;
        }

        private IEnumerable<HealthEvent> GenerateWeaklyEvents(int days, DateTime startDate, IEnumerable<DaysOfWeek?> daysOfWeek)
        {
            var events = new List<HealthEvent>();
            var daysOfWeeks = daysOfWeek as DaysOfWeek?[] ?? daysOfWeek.ToArray();
            for (var i = 0; i < days; i++)
            {
                var day = startDate.AddDays(i).DayOfWeek;
                if (daysOfWeeks.Any(item => item.ToDayOfWeek() == day))
                {
                    events.AddRange(this.GenerateEventsOnSingleFrequency(startDate.AddDays(i)));
                }
            }

            return events;
        }

        private IEnumerable<HealthEvent> GenerateEventsOnSingleFrequency(DateTime date)
        {
            var events = new List<HealthEvent>();
            if (this.timing.Repeat.TimeOfDay.Any())
            {
                foreach (var time in this.timing.Repeat.TimeOfDay)
                {
                    var eventDateTime = DateTime.Parse($"{date.Date:yyyy-MM-dd}T{time}");
                    var healthEvent = new HealthEvent
                    {
                        PatientId = this.patient.Id,
                        EventDateTime = eventDateTime,
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

                    // Default hour will be 0. Thus, patient will be asked to set a custom timing value to get these
                    // events (and, consequently, events will be updated).
                    var eventDate = exactTimeIsSetup
                        ? date.Date
                            .AddHours(patient.ExactEventTimes[customTiming].Hour)
                            .AddMinutes(patient.ExactEventTimes[customTiming].Minute)
                        : date.Date;
                    var healthEvent = new HealthEvent
                    {
                        PatientId = this.patient.Id,
                        EventDateTime = eventDate,
                        ExactTimeIsSetup = exactTimeIsSetup,
                        EventTiming = customTiming,
                        ResourceReference = this.resourceReference
                    };
                    events.Add(healthEvent);
                }
            }
            else
            {
                throw new InvalidOperationException("Dosage does not have a valid frequency by day");
            }
            
            return events;
        }

        private IEnumerable<HealthEvent> GenerateEventsOnMultipleFrequency(int days, DateTime startDate)
        {
            var events = new List<HealthEvent>();
            var totalOccurrences = days * this.timing.Repeat.Frequency ?? 0;
            var hourOfDistance = 24 / this.timing.Repeat.Frequency ?? 24;
            for (var i = 0; i < totalOccurrences; i++)
            {
                var date = startDate.AddHours(hourOfDistance * i);
                var healthEvent = new HealthEvent
                {
                    PatientId = this.patient.Id,
                    EventDateTime = date,
                    ExactTimeIsSetup = true,
                    EventTiming = CustomEventTiming.EXACT,
                    ResourceReference = this.resourceReference
                };
                events.Add(healthEvent);
            }

            return events;
        }
    }
}