namespace QMUL.DiabetesBackend.ServiceImpl.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Hl7.Fhir.Model;
    using Model;
    using Model.Enums;
    using Patient = Model.Patient;

    /// <summary>
    /// Generates health events based on a Resource frequency.
    /// </summary>
    public class EventsGenerator
    {
        private readonly Patient patient;
        private readonly Timing timing;
        private readonly CustomResource referenceResource;

        private EventsGenerator(Patient patient, Timing timing, CustomResource referenceResource)
        {
            this.patient = patient;
            this.timing = timing;
            this.referenceResource = referenceResource;
        }
        
        /// <summary>
        /// Creates a list of events based on medication timings.
        /// </summary>
        /// <param name="request">The medication request</param>
        /// <param name="patient">The medication request's subject</param>
        /// <returns>A List of events for the medication request</returns>
        public static IEnumerable<HealthEvent> GenerateEventsFrom(MedicationRequest request, Patient patient)
        {
            var events = new List<HealthEvent>();
            var isInsulin = ResourceUtils.IsInsulinResource(request);

            foreach (var dosage in request.DosageInstruction)
            {
                var requestReference = new CustomResource
                {
                    EventType = isInsulin ? EventType.InsulinDosage : EventType.MedicationDosage,
                    ResourceId = request.Id,
                    Text = dosage.Text,
                    EventReferenceId = dosage.ElementId
                };
                var eventsGenerator = new EventsGenerator(patient, dosage.Timing, requestReference);
                events.AddRange(eventsGenerator.GetEvents());
            }

            return events;
        }

        /// <summary>
        /// Creates a list of <see cref="HealthEvent"/> from a <see cref="ServiceRequest"/>
        /// </summary>
        /// <param name="request">The medication request</param>
        /// <param name="patient">The medication request's subject</param>
        /// <returns>A List of events for the medication request</returns>
        public static IEnumerable<HealthEvent> GenerateEventsFrom(ServiceRequest request, Patient patient)
        {
            var events = new List<HealthEvent>();
            var requestReference = new CustomResource
            {
                EventType = EventType.Measurement,
                ResourceId = request.Id,
                EventReferenceId = request.Id,
                Text = request.PatientInstruction
            };

            if (request.Occurrence is not Timing timing)
            {
                return events;
            }

            var eventsGenerator = new EventsGenerator(patient, timing, requestReference);
            events.AddRange(eventsGenerator.GetEvents());
            return events;
        }

        private IEnumerable<HealthEvent> GetEvents()
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
                    startDate = this.GetPatientTimeOrDefault();
                    break;
                default:
                    throw new InvalidOperationException("Dosage does not have a valid timing");
            }

            if (timing.Repeat.DayOfWeek.Any())
            {
                return this.GenerateWeaklyEvents(days, startDate, timing.Repeat.DayOfWeek);
            }

            return timing.Repeat.Period switch
            {
                1 when timing.Repeat.PeriodUnit == Timing.UnitsOfTime.D && timing.Repeat.Frequency > 1 => this
                    .GenerateEventsOnMultipleFrequency(days, startDate),
                1 when timing.Repeat.PeriodUnit == Timing.UnitsOfTime.D => this.GenerateDailyEvents(days, startDate),
                _ => throw new InvalidOperationException("Dosage timing not supported yet")
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
                        Resource = this.referenceResource
                    };
                    events.Add(healthEvent);
                }
            }
            else if (this.timing.Repeat.When.Any())
            {
                foreach (var eventTiming in this.timing.Repeat.When)
                {
                    var customTiming = eventTiming.ToCustomEventTiming();
                    var valueExists = patient.ExactEventTimes.ContainsKey(customTiming);
                    var eventDate = valueExists
                        ? date.Date
                            .AddHours(patient.ExactEventTimes[customTiming].Hour)
                            .AddMinutes(patient.ExactEventTimes[customTiming].Minute)
                        : date.Date;
                    var healthEvent = new HealthEvent
                    {
                        PatientId = this.patient.Id,
                        EventDateTime = eventDate,
                        ExactTimeIsSetup = valueExists,
                        EventTiming = customTiming,
                        Resource = this.referenceResource
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

        private DateTime GetPatientTimeOrDefault()
        {
            var startTimeExits = patient.ResourceStartDate.ContainsKey(this.referenceResource.EventReferenceId);
            return startTimeExits
                ? patient.ResourceStartDate[this.referenceResource.EventReferenceId]
                : DateTime.UtcNow;
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
                    Resource = this.referenceResource
                };
                events.Add(healthEvent);
            }

            return events;
        }
    }
}