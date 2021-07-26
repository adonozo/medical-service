using System;
using System.Collections.Generic;
using System.Linq;
using Hl7.Fhir.Model;
using QMUL.DiabetesBackend.Model;
using QMUL.DiabetesBackend.Model.Enums;

namespace QMUL.DiabetesBackend.ServiceImpl.Utils
{
    public class EventsGenerator
    {
        private readonly string patientId;
        private readonly Timing timing;
        private readonly CustomResource referenceResource;

        public EventsGenerator(string patientId, Timing timing, CustomResource referenceResource)
        {
            this.patientId = patientId;
            this.timing = timing;
            this.referenceResource = referenceResource;
        }

        public List<HealthEvent> GetEvents()
        {
            int days;
            DateTime startDate;
            switch (timing.Repeat.Bounds)
            {
                case Period bounds:
                {
                    startDate = DateTime.Parse(bounds.Start);
                    var endDate = DateTime.Parse(bounds.End);
                    days = (endDate - startDate).Days;
                    break;
                }
                case Duration {Code: "d"} duration:
                    days = duration.Value == null
                        ? throw new InvalidOperationException("Duration is not defined")
                        : (int) duration.Value;
                    startDate = DateTime.Now;
                    // TODO Assuming a startDate = Now; should ask user first.
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

        private List<HealthEvent> GenerateWeaklyEvents(int days, DateTime startDate, DaysOfWeek?[] daysOfWeek)
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

        private List<HealthEvent> GenerateEventsOnFrequency(DateTime date)
        {
            var events = new List<HealthEvent>();
            if (this.timing.Repeat.TimeOfDay.Any())
            {
                foreach (var time in this.timing.Repeat.TimeOfDay)
                {
                    var eventDateTime = DateTime.Parse($"{date.Date:yyyy-MM-dd}T{time}");
                    var healthEvent = new HealthEvent
                    {
                        PatientId = this.patientId,
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
                // TODO still need a exact time from the patient
                foreach (var eventTiming in this.timing.Repeat.When)
                {
                    var healthEvent = new HealthEvent
                    {
                        PatientId = this.patientId,
                        EventDateTime = date.Date,
                        ExactTimeIsSetup = false,
                        EventTiming = eventTiming.ToCustomEventTiming(),
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