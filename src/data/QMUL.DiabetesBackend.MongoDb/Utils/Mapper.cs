namespace QMUL.DiabetesBackend.MongoDb.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Hl7.Fhir.Model;
    using Model;
    using Models;
    using static System.Enum;
    using ResourceReference = Hl7.Fhir.Model.ResourceReference;

    /// <summary>
    /// Maps FHIR objects into custom Mongo objects and vice-versa.
    /// </summary>
    public static class Mapper
    {
        public static Timing ToTiming(this MongoTiming timing)
        {
            var hasPeriodUnit = TryParse<Timing.UnitsOfTime>(timing.PeriodUnit, out var periodUnit);
            var dayEvent = new List<Timing.EventTiming?>();
            foreach (var value in timing.When)
            {
                var canParse = TryParse<Timing.EventTiming>(value, out var result);
                if (canParse)
                {
                    dayEvent.Add(result);
                }
            }

            var daysOfWeek = new List<DaysOfWeek?>();
            foreach (var day in timing.DaysOfWeek)
            {
                var canParse = TryParse<DaysOfWeek>(day, out var result);
                if (canParse)
                {
                    daysOfWeek.Add(result);
                }
            }

            DataType bounds;
            if (timing.PeriodStart != null && timing.PeriodEnd != null)
            {
                bounds = new Period
                {
                    Start = timing.PeriodStart?.ToString("o"),
                    End = timing.PeriodEnd?.ToString("o")
                };
            }
            else
            {
                bounds = new Duration
                {
                    Value = timing.DayDuration,
                    Unit = "d"
                };
            }

            return new Timing
            {
                Repeat = new Timing.RepeatComponent
                {
                    Bounds = bounds,
                    Frequency = timing.Frequency,
                    Period = timing.Period,
                    PeriodUnit = hasPeriodUnit ? periodUnit : null,
                    Offset = timing.Offset,
                    When = dayEvent,
                    TimeOfDay = timing.TimesOfDay,
                    DayOfWeek = daysOfWeek
                }
            };
        }

        public static Quantity ToQuantity(this MongoQuantity quantity) =>
            new()
            {
                System = quantity.System,
                Code = quantity.Code,
                Unit = quantity.Unit,
                Value = quantity.Value,
            };

        public static MongoTiming ToMongoTiming(this Timing timing)
        {
            var startTime = (timing.Repeat.Bounds as Period)?.Start;
            var endTime = (timing.Repeat.Bounds as Period)?.End;
            var days = 0;
            if (timing.Repeat.Bounds is Duration duration)
            {
                days = duration.Value != null ? (int)duration.Value : 0;
            }

            var whenList = from eventTiming in timing.Repeat.When
                where eventTiming != null
                select eventTiming.ToString();
            var daysOfWeek = from day in timing.Repeat.DayOfWeek
                where day != null
                select day.ToString();

            return new MongoTiming
            {
                Frequency = timing.Repeat.Frequency ?? 0,
                Period = timing.Repeat.Period ?? 0,
                PeriodUnit = timing.Repeat.PeriodUnit?.ToString(),
                DayDuration = days,
                Offset = timing.Repeat.Offset ?? 0,
                PeriodStart = startTime != null ? DateTime.Parse(startTime) : null,
                PeriodEnd = endTime != null ? DateTime.Parse(endTime) : null,
                When = whenList,
                DaysOfWeek = daysOfWeek,
                TimesOfDay = timing.Repeat.TimeOfDay.Select(time => time.ToString())
            };
        }

        public static MongoQuantity ToMongoQuantity(this Quantity dose)
        {
            return new()
            {
                Value = dose.Value ?? 0,
                System = dose.System,
                Unit = dose.Unit,
                Code = dose.Code
            };
        }

        public static MongoEvent ToMongoEvent(this HealthEvent healthEvent)
        {
            return new()
            {
                Id = healthEvent.Id,
                PatientId = healthEvent.PatientId,
                EventDateTime = healthEvent.EventDateTime,
                EventTiming = healthEvent.EventTiming,
                ExactTimeIsSetup = healthEvent.ExactTimeIsSetup,
                ResourceReference = healthEvent.ResourceReference
            };
        }

        public static HealthEvent ToHealthEvent(this MongoEvent mongoEvent)
        {
            return new()
            {
                Id = mongoEvent.Id,
                PatientId = mongoEvent.PatientId,
                EventDateTime = mongoEvent.EventDateTime,
                EventTiming = mongoEvent.EventTiming,
                ExactTimeIsSetup = mongoEvent.ExactTimeIsSetup,
                ResourceReference = mongoEvent.ResourceReference
            };
        }

        public static MongoCode ToMongoCode(this Coding code)
        {
            return new()
            {
                Display = code.Display,
                Code = code.Code,
                System = code.System
            };
        }

        public static Coding ToCoding(this MongoCode code)
        {
            return new()
            {
                Display = code.Display,
                Code = code.Code,
                System = code.System
            };
        }

        public static ResourceReference ToResourceReference(this MongoReference reference)
        {
            return new()
            {
                ElementId = reference.ReferenceId,
                Display = reference.ReferenceName,
            };
        }
    }
}