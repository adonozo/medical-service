namespace QMUL.DiabetesBackend.ServiceImpl.Utils
{
    using System;
    using Hl7.Fhir.Model;
    using Model.Enums;
    using NodaTime;
    using Duration = NodaTime.Duration;
    using Instant = NodaTime.Instant;
    using Patient = Model.Patient;

    /// <summary>
    /// Has helper methods to map Timing objects
    /// </summary>
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

        /// <summary>
        /// Gets a time interval for a timing event, considering the patient's timezone and previous timing personal
        /// settings. If the patient do not have a datetime for the timing event, a default time is used. E.g., if the
        /// patient have not declared the time for breakfast before, it will use a default time. 
        /// </summary>
        /// <param name="patient">The patient to get the time interval for.</param>
        /// <param name="startTime">The reference start date for the interval.</param>
        /// <param name="timing">The timing event.</param>
        /// <param name="timezone">The patient's timezone.</param>
        /// <param name="defaultOffset">An offset for the time interval, in minutes.</param>
        /// <returns>A <see cref="DateTime"/> Tuple with the start and end datetime.</returns>
        public static Tuple<DateTime, DateTime> GetIntervalForPatient(Patient patient, DateTime startTime,
            CustomEventTiming timing, string timezone, int defaultOffset)
        {
            DateTime start;
            DateTime end;
            if (patient.ExactEventTimes.ContainsKey(timing))
            {
                start = patient.ExactEventTimes[timing].AddMinutes(defaultOffset * -1);
                start = startTime.Date.AddHours(start.Hour).AddMinutes(start.Minute);
                end = patient.ExactEventTimes[timing].AddMinutes(defaultOffset);
                end = startTime.Date.AddHours(end.Hour).AddMinutes(end.Minute);
            }
            else
            {
                (start, end) = GetIntervalFromCustomEventTiming(startTime, timing, timezone);
            }

            return new Tuple<DateTime, DateTime>(start, end);
        }

        /// <summary>
        /// Gets the default interval time for a Timing event, considering the patient's timezone.
        /// </summary>
        /// <param name="startTime">The reference start date for the interval.</param>
        /// <param name="timing">The timing event.</param>
        /// <param name="timezone">The patient's timezone.</param>
        /// <returns>A <see cref="DateTime"/> Tuple with the start and end datetime.</returns>
        /// <exception cref="ArgumentOutOfRangeException">If there is no a default value for the timing event.</exception>
        public static Tuple<DateTime, DateTime> GetIntervalFromCustomEventTiming(DateTime startTime, CustomEventTiming timing, string timezone)
        {
            DateTime endTime;
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

            return new Tuple<DateTime, DateTime>(startTime, endTime);
        }

        /// <summary>
        /// Gets the related timing events for a given event. E.g, A before-breakfast event will get all breakfast
        /// related events.
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

        public static Tuple<DateTime, DateTime> GetRelativeDayInterval(DateTime date, string timezone)
        {
            var startLocalDate = GetRelativeStartOfDay(date, timezone);
            date = startLocalDate.ToDateTimeUtc();
            var endTime = date.AddDays(1);
            return new Tuple<DateTime, DateTime>(date, endTime);
        }

        private static ZonedDateTime GetRelativeStartOfDay(DateTime date, string timezone)
        {
            if (date.Kind != DateTimeKind.Utc)
            {
                date = date.ToUniversalTime();
            }

            var instant = Instant.FromDateTimeUtc(date);
            var timezoneInfo = DateTimeZoneProviders.Tzdb[timezone];
            var startZonedDateTime = instant.InZone(timezoneInfo);
            var startLocalDate = startZonedDateTime.Date.AtStartOfDayInZone(timezoneInfo);
            return startLocalDate;
        }
    }
}