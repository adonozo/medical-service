using System;
using System.Collections.Generic;
using System.Linq;
using Hl7.Fhir.Model;
using QMUL.DiabetesBackend.Model;
using QMUL.DiabetesBackend.Model.Enums;
using Patient = QMUL.DiabetesBackend.Model.Patient;

namespace QMUL.DiabetesBackend.ServiceImpl.Utils
{
    public class EventsGenerator
    {
        private readonly Patient patient;
        private readonly Timing timing;
        private readonly CustomResource referenceResource;

        public EventsGenerator(Patient patient, Timing timing, CustomResource referenceResource)
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
                    var startTimeExits = patient.ResourceStartDate.ContainsKey(this.referenceResource.EventReferenceId);
                    startDate = startTimeExits
                        ? patient.ResourceStartDate[this.referenceResource.EventReferenceId]
                        : DateTime.UtcNow;
                    break;
                default:
                    throw new InvalidOperationException("Dosage does not have a valid timing");
            }

            if (timing.Repeat.Period == 1 && timing.Repeat.PeriodUnit == Timing.UnitsOfTime.D)
            {
                return this.GenerateDailyEvents(days, startDate);
            }

            if (timing.Repeat.DayOfWeek.Any())
            {
                return this.GenerateWeaklyEvents(days, startDate, timing.Repeat.DayOfWeek as DaysOfWeek?[]);
            }
            
            throw new InvalidOperationException("Dosage timing not supported yet");
        }

        private List<HealthEvent> GenerateDailyEvents(int days, DateTime startDate)
        {
            var events = new List<HealthEvent>();
            for (var i = 0; i < days; i++)
            {
                var day = startDate.AddDays(i);
                events.AddRange(this.GenerateEventsOnFrequency(day));
            }

            return events;
        }

        private IEnumerable<HealthEvent> GenerateWeaklyEvents(int days, DateTime startDate, DaysOfWeek?[] daysOfWeek)
        {
            var events = new List<HealthEvent>();
            for (var i = 0; i < days; i++)
            {
                var day = startDate.AddDays(i).DayOfWeek;
                if (Array.Exists(daysOfWeek, item => item.ToDayOfWeek() == day))
                {
                    events.AddRange(this.GenerateEventsOnFrequency(startDate));
                }
            }

            return events;
        }

        private IEnumerable<HealthEvent> GenerateEventsOnFrequency(DateTime date)
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
            // TODO missing once a day, twice a day, etc... Also needs a exact time from the patient
            else
            {
                throw new InvalidOperationException("Dosage does not have a valid frequency by day");
            }
            
            return events;
        }
    }
}