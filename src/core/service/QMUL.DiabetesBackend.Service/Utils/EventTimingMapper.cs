namespace QMUL.DiabetesBackend.Service.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Hl7.Fhir.Model;
    using Model.Enums;
    using NodaTime;
    using NodaTime.Extensions;
    using Duration = NodaTime.Duration;

    /// <summary>
    /// Helper methods to map Timing objects
    /// </summary>
    public static class EventTimingMapper
    {
        [ExcludeFromCodeCoverage]
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

        /// <summary>
        /// Gets a time interval for a timing event, considering the patient's timezone and previous timing personal
        /// settings. If the patient does not have a datetime for the timing event, a default time is used, e.g., if the
        /// patient have not declared the time for breakfast before, it will use a default time. 
        /// </summary>
        /// <param name="preferences">The patient's timing preferences in a <see cref="Dictionary{TKey,TValue}"/>.</param>
        /// <param name="startTime">The reference start date for the interval.</param>
        /// <param name="timing">The timing event.</param>
        /// <param name="timezone">The patient's timezone.</param>
        /// <param name="defaultOffset">An offset for the time interval, in minutes.</param>
        /// <returns>A <see cref="DateTime"/> Tuple with the start and end datetime.</returns>
        public static Tuple<DateTimeOffset, DateTimeOffset> GetIntervalForPatient(Dictionary<CustomEventTiming, DateTimeOffset> preferences, 
            DateTimeOffset startTime, CustomEventTiming timing, string timezone, int defaultOffset)
        {
            DateTimeOffset start;
            DateTimeOffset end;
            if (preferences.ContainsKey(timing))
            {
                start = preferences[timing].AddMinutes(defaultOffset * -1);
                start = startTime.Date.AddHours(start.Hour).AddMinutes(start.Minute);
                end = preferences[timing].AddMinutes(defaultOffset);
                end = startTime.Date.AddHours(end.Hour).AddMinutes(end.Minute);
            }
            else
            {
                return GetIntervalFromCustomEventTiming(startTime, timing, timezone);
            }

            return new Tuple<DateTimeOffset, DateTimeOffset>(start, end);
        }

        /// <summary>
        /// Gets the default interval time for a Timing event, considering the patient's timezone.
        /// </summary>
        /// <param name="startTime">The reference start date for the interval.</param>
        /// <param name="timing">The timing event.</param>
        /// <param name="timezone">The patient's timezone.</param>
        /// <returns>A <see cref="DateTime"/> Tuple with the start and end datetime.</returns>
        /// <exception cref="ArgumentOutOfRangeException">If there is not a default value for the timing event.</exception>
        public static Tuple<DateTimeOffset, DateTimeOffset> GetIntervalFromCustomEventTiming(DateTimeOffset startTime, CustomEventTiming timing, string timezone)
        {
            DateTimeOffset endTime;
            var startLocalDate = GetRelativeStartOfDay(startTime, timezone);
            
            switch (timing)
            {
                case CustomEventTiming.MORN:
                    startTime = startLocalDate.Plus(Duration.FromHours(6)).ToDateTimeUtc();
                    endTime = startTime.AddHours(6);
                    break;
                case CustomEventTiming.MORN_early:
                    startTime = startLocalDate.Plus(Duration.FromHours(6)).ToDateTimeUtc();
                    endTime = startTime.AddHours(3);
                    break;
                case CustomEventTiming.MORN_late:
                    startTime = startLocalDate.Plus(Duration.FromHours(9)).ToDateTimeUtc();
                    endTime = startTime.AddHours(3);
                    break;
                case CustomEventTiming.NOON:
                    startTime = startLocalDate.Plus(Duration.FromHours(11)).PlusMinutes(30).ToDateTimeUtc();
                    endTime = startTime.AddHours(1);
                    break;
                case CustomEventTiming.AFT:
                    startTime = startLocalDate.Plus(Duration.FromHours(12)).ToDateTimeUtc();
                    endTime = startTime.AddHours(6);
                    break;
                case CustomEventTiming.AFT_early:
                    startTime = startLocalDate.Plus(Duration.FromHours(12)).ToDateTimeUtc();
                    endTime = startTime.AddHours(3);
                    break;
                case CustomEventTiming.AFT_late:
                    startTime = startLocalDate.Plus(Duration.FromHours(15)).ToDateTimeUtc();
                    endTime = startTime.AddHours(3);
                    break;
                case CustomEventTiming.EVE:
                    startTime = startLocalDate.Plus(Duration.FromHours(18)).ToDateTimeUtc();
                    endTime = startTime.AddHours(6);
                    break;
                case CustomEventTiming.EVE_early:
                    startTime = startLocalDate.Plus(Duration.FromHours(18)).ToDateTimeUtc();
                    endTime = startTime.AddHours(3);
                    break;
                case CustomEventTiming.EVE_late:
                    startTime = startLocalDate.Plus(Duration.FromHours(21)).ToDateTimeUtc();
                    endTime = startTime.AddHours(3);
                    break;
                case CustomEventTiming.NIGHT:
                    startTime = startLocalDate.Plus(Duration.FromHours(18)).ToDateTimeUtc();
                    endTime = startTime.AddHours(12);
                    break;
                case CustomEventTiming.ACM:
                case CustomEventTiming.CM:
                case CustomEventTiming.PCM:
                    startTime = startLocalDate.Plus(Duration.FromHours(6)).ToDateTimeUtc();
                    endTime = startTime.AddHours(5);
                    break;
                case CustomEventTiming.ACD:
                case CustomEventTiming.CD:
                case CustomEventTiming.PCD:
                    startTime = startLocalDate.Plus(Duration.FromHours(11)).ToDateTimeUtc();
                    endTime = startTime.AddHours(6);
                    break;
                case CustomEventTiming.ACV:
                case CustomEventTiming.CV:
                case CustomEventTiming.PCV:
                    startTime = startLocalDate.Plus(Duration.FromHours(17)).ToDateTimeUtc();
                    endTime = startTime.AddHours(7);
                    break;
                case CustomEventTiming.ALL_DAY:
                    startTime = startLocalDate.ToDateTimeUtc();
                    endTime = startTime.AddDays(1);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(timing), timing, null);
            }

            return new Tuple<DateTimeOffset, DateTimeOffset>(startTime, endTime);
        }

        /// <summary>
        /// Gets the related timing events for a given event. E.g, For before-breakfast, it will get all breakfast
        /// related events: before, during, and after breakfast.
        /// </summary>
        /// <param name="eventTiming">The custom timing event.</param>
        /// <returns>An array with the related timing events. Empty if there are no related events.</returns>
        public static CustomEventTiming[] GetRelatedTimings(CustomEventTiming eventTiming)
        {
            return eventTiming switch
            {
                CustomEventTiming.ACM => new[] {CustomEventTiming.ACM, CustomEventTiming.CM, CustomEventTiming.PCM},
                CustomEventTiming.CM => new[] {CustomEventTiming.ACM, CustomEventTiming.CM, CustomEventTiming.PCM},
                CustomEventTiming.PCM => new[] {CustomEventTiming.ACM, CustomEventTiming.CM, CustomEventTiming.PCM},
                CustomEventTiming.ACD => new[] {CustomEventTiming.ACD, CustomEventTiming.CD, CustomEventTiming.PCD},
                CustomEventTiming.CD => new[] {CustomEventTiming.ACD, CustomEventTiming.CD, CustomEventTiming.PCD},
                CustomEventTiming.PCD => new[] {CustomEventTiming.ACD, CustomEventTiming.CD, CustomEventTiming.PCD},
                CustomEventTiming.ACV => new[] {CustomEventTiming.ACV, CustomEventTiming.CV, CustomEventTiming.PCV},
                CustomEventTiming.CV => new[] {CustomEventTiming.ACV, CustomEventTiming.CV, CustomEventTiming.PCV},
                CustomEventTiming.PCV => new[] {CustomEventTiming.ACV, CustomEventTiming.CV, CustomEventTiming.PCV},
                _ => Array.Empty<CustomEventTiming>()
            };
        }

        /// <summary>
        /// Gets a day interval for a date given a timezone. The result dates are calculated in UTC. For example, the
        /// for a datetime 2020-01-01 10:00 America/La_Paz, the intervals would be 2020-01-01 04:00 & 2020-01-02 04:00
        /// considering the timezone (UTC -4). 
        /// </summary>
        /// <param name="date">The reference datetime.</param>
        /// <param name="timezone">The user's timezone.</param>
        /// <returns>A <see cref="DateTime"/> tuple: the start and end day of the reference date in UTC.</returns>
        public static Tuple<DateTimeOffset, DateTimeOffset> GetRelativeDayInterval(DateTimeOffset date, string timezone)
        {
            var startLocalDate = GetRelativeStartOfDay(date, timezone);
            date = startLocalDate.ToDateTimeUtc();
            var endTime = date.AddDays(1);
            return new Tuple<DateTimeOffset, DateTimeOffset>(date, endTime);
        }

        /// <summary>
        /// Gets the the relative start of day (datetime) given a date and timezone.
        /// </summary>
        /// <param name="date">The reference date for get the start date from.</param>
        /// <param name="timezone">The user's timezone</param>
        /// <returns>A <see cref="ZonedDateTime"/> at the same date of the reference date, but the time set to 00:00.</returns>
        public static ZonedDateTime GetRelativeStartOfDay(DateTimeOffset date, string timezone)
        {
            var instant = date.ToInstant();
            var timezoneInfo = DateTimeZoneProviders.Tzdb[timezone];
            var startZonedDateTime = instant.InZone(timezoneInfo);
            var startLocalDate = startZonedDateTime.Date.AtStartOfDayInZone(timezoneInfo);
            return startLocalDate;
        }
    }
}