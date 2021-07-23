using System;
using Hl7.Fhir.Model;
using QMUL.DiabetesBackend.Model.Enums;

namespace QMUL.DiabetesBackend.ServiceImpl.Utils
{
    public static class EventTimingMapper
    {
        public static CustomEventTiming ToCustomEventTiming(this Timing.EventTiming? eventTiming)
        {
            return eventTiming switch
            {
                Timing.EventTiming.MORN => CustomEventTiming.MORN,
                Timing.EventTiming.MORN_early => CustomEventTiming.MORN_early,
                Timing.EventTiming.MORN_late => CustomEventTiming.MORN_late,
                Timing.EventTiming.NOON => CustomEventTiming.NOON,
                Timing.EventTiming.AFT => CustomEventTiming.AFT,
                Timing.EventTiming.AFT_early => CustomEventTiming.AFT_early,
                Timing.EventTiming.AFT_late => CustomEventTiming.AFT_late,
                Timing.EventTiming.EVE => CustomEventTiming.EVE,
                Timing.EventTiming.EVE_early => CustomEventTiming.EVE_early,
                Timing.EventTiming.EVE_late => CustomEventTiming.EVE_late,
                Timing.EventTiming.NIGHT => CustomEventTiming.NIGHT,
                Timing.EventTiming.PHS => CustomEventTiming.PHS,
                Timing.EventTiming.HS => CustomEventTiming.HS,
                Timing.EventTiming.WAKE => CustomEventTiming.WAKE,
                Timing.EventTiming.C => CustomEventTiming.C,
                Timing.EventTiming.CM => CustomEventTiming.CM,
                Timing.EventTiming.CD => CustomEventTiming.CD,
                Timing.EventTiming.CV => CustomEventTiming.CV,
                Timing.EventTiming.AC => CustomEventTiming.AC,
                Timing.EventTiming.ACM => CustomEventTiming.ACM,
                Timing.EventTiming.ACD => CustomEventTiming.ACD,
                Timing.EventTiming.ACV => CustomEventTiming.ACV,
                Timing.EventTiming.PC => CustomEventTiming.PC,
                Timing.EventTiming.PCM => CustomEventTiming.PCM,
                Timing.EventTiming.PCD => CustomEventTiming.PCD,
                Timing.EventTiming.PCV => CustomEventTiming.PCV,
                _ => default
            };
        }

        public static DayOfWeek ToDayOfWeek(this DaysOfWeek? dayOfWeek)
        {
            return dayOfWeek switch
            {
                DaysOfWeek.Mon => DayOfWeek.Monday,
                DaysOfWeek.Tue => DayOfWeek.Tuesday,
                DaysOfWeek.Wed => DayOfWeek.Wednesday,
                DaysOfWeek.Thu => DayOfWeek.Thursday,
                DaysOfWeek.Fri => DayOfWeek.Friday,
                DaysOfWeek.Sat => DayOfWeek.Saturday,
                DaysOfWeek.Sun => DayOfWeek.Sunday,
                null => throw new ArgumentOutOfRangeException(nameof(dayOfWeek), "Invalid day of week"),
                _ => throw new ArgumentOutOfRangeException(nameof(dayOfWeek), dayOfWeek, "Invalid day of week")
            };
        }
    }
}