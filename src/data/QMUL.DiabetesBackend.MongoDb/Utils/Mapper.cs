using System;
using System.Collections.Generic;
using System.Linq;
using Hl7.Fhir.Model;
using QMUL.DiabetesBackend.Model;
using QMUL.DiabetesBackend.MongoDb.Models;
using static System.Enum;

namespace QMUL.DiabetesBackend.MongoDb.Utils
{
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

            return new Timing
            {
                Repeat = new Timing.RepeatComponent
                {
                    Bounds = new Period
                    {
                        Start = timing.PeriodStartTime.ToString("yyyy-MM-dd"),
                        End = timing.PeriodEndTime.ToString("yyyy-MM-dd")
                    },
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

        public static Quantity ToQuantity(this MongoQuantity doseAndRate) =>
            new()
            {
                System = doseAndRate.System,
                Code = doseAndRate.Code,
                Unit = doseAndRate.Unit,
                Value = doseAndRate.Value,
            };

        public static MongoTiming ToMongoTiming(this Timing timing)
        {
            var startTime = (timing.Repeat.Bounds as Period)?.Start;
            var endTime = (timing.Repeat.Bounds as Period)?.End;
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
                Offset = timing.Repeat.Offset ?? 0,
                PeriodStartTime = DateTime.Parse(startTime ?? string.Empty),
                PeriodEndTime = DateTime.Parse(endTime ?? string.Empty),
                When = whenList,
                DaysOfWeek = daysOfWeek,
                TimesOfDay = timing.Repeat.TimeOfDay.Select(time => time.ToString())
            };
        }

        public static MongoQuantity ToMongoQuantity(this Dosage.DoseAndRateComponent dose)
        {
            return new()
            {
                Value = ((Quantity) dose.Dose).Value ?? 0,
                System = ((Quantity) dose.Dose).System,
                Unit = ((Quantity) dose.Dose).Unit,
                Code = ((Quantity) dose.Dose).Code
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
                Resource = healthEvent.Resource
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
                Resource = mongoEvent.Resource
            };
        }
    }
}