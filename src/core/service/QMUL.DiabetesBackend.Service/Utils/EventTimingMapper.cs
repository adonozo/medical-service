namespace QMUL.DiabetesBackend.Service.Utils;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Hl7.Fhir.Model;
using Model.Enums;
using NodaTime;
using NodaTime.Extensions;
using Instant = NodaTime.Instant;
using Period = NodaTime.Period;

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
    /// <param name="dateTime">The reference start date for the interval.</param>
    /// <param name="timing">The timing event.</param>
    /// <param name="timezone">The patient's timezone.</param>
    /// <param name="defaultOffset">An offset for the time interval, in minutes.</param>
    /// <returns>An <see cref="Instant"/> Tuple with the start and end datetime.</returns>
    public static (Instant Start, Instant End) GetTimingInterval(
        Dictionary<CustomEventTiming, LocalTime> preferences,
        LocalDate dateTime,
        CustomEventTiming timing,
        string timezone,
        int defaultOffset)
    {
        if (timing == CustomEventTiming.EXACT)
        {
            var start = dateTime.AtStartOfDayInZone(DateTimeZone.Utc).ToInstant();
            var end = dateTime.Plus(Period.FromDays(1)).AtStartOfDayInZone(DateTimeZone.Utc).ToInstant();
            return (start, end);
        }

        if (preferences.ContainsKey(timing))
        {
            var start = dateTime.At(preferences[timing]).Plus(Period.FromMinutes(defaultOffset * -1));
            var end = dateTime.At(preferences[timing]).Plus(Period.FromMinutes(defaultOffset));
            return (start.InUtc().ToInstant(), end.InUtc().ToInstant());
        }
        else
        {
            return GetIntervalFromCustomEventTiming(dateTime, timing, timezone);
        }
    }

    /// <summary>
    /// Gets the default interval time for a Timing event, considering the patient's timezone.
    /// </summary>
    /// <param name="dateTime">The reference start date for the interval.</param>
    /// <param name="timing">The timing event.</param>
    /// <param name="timezone">The patient's timezone.</param>
    /// <returns>An <see cref="Instant"/> Tuple with the start and end datetime.</returns>
    /// <exception cref="ArgumentOutOfRangeException">If there is not a default value for the timing event.</exception>
    public static (Instant Start, Instant End) GetIntervalFromCustomEventTiming(LocalDate dateTime,
        CustomEventTiming timing,
        string timezone)
    {
        var zone = DateTimeZoneProviders.Tzdb[timezone];
        var startLocalDate = dateTime.AtStartOfDayInZone(zone);

        switch (timing)
        {
            case CustomEventTiming.MORN:
                return (startLocalDate.PlusHours(6).ToInstant(), startLocalDate.PlusHours(12).ToInstant());
            case CustomEventTiming.MORN_early:
                return (startLocalDate.PlusHours(6).ToInstant(), startLocalDate.PlusHours(9).ToInstant());
            case CustomEventTiming.MORN_late:
                return (startLocalDate.PlusHours(9).ToInstant(), startLocalDate.PlusHours(12).ToInstant());
            case CustomEventTiming.NOON:
                return (startLocalDate.PlusHours(11).PlusMinutes(30).ToInstant(),
                    startLocalDate.PlusHours(12).PlusMinutes(30).ToInstant());
            case CustomEventTiming.AFT:
                return (startLocalDate.PlusHours(12).ToInstant(), startLocalDate.PlusHours(18).ToInstant());
            case CustomEventTiming.AFT_early:
                return (startLocalDate.PlusHours(12).ToInstant(), startLocalDate.PlusHours(15).ToInstant());
            case CustomEventTiming.AFT_late:
                return (startLocalDate.PlusHours(15).ToInstant(), startLocalDate.PlusHours(18).ToInstant());
            case CustomEventTiming.EVE:
                return (startLocalDate.PlusHours(18).ToInstant(), startLocalDate.PlusHours(24).ToInstant());
            case CustomEventTiming.EVE_early:
                return (startLocalDate.PlusHours(18).ToInstant(), startLocalDate.PlusHours(21).ToInstant());
            case CustomEventTiming.EVE_late:
                return (startLocalDate.PlusHours(21).ToInstant(), startLocalDate.PlusHours(24).ToInstant());
            case CustomEventTiming.NIGHT:
                return (startLocalDate.PlusHours(18).ToInstant(),
                    startLocalDate.PlusHours(27).ToInstant());
            case CustomEventTiming.ACM:
            case CustomEventTiming.CM:
            case CustomEventTiming.PCM:
                return (startLocalDate.PlusHours(6).ToInstant(), startLocalDate.PlusHours(11).ToInstant());
            case CustomEventTiming.ACD:
            case CustomEventTiming.CD:
            case CustomEventTiming.PCD:
                return (startLocalDate.PlusHours(11).ToInstant(), startLocalDate.PlusHours(17).ToInstant());
            case CustomEventTiming.ACV:
            case CustomEventTiming.CV:
            case CustomEventTiming.PCV:
                return (startLocalDate.PlusHours(17).ToInstant(), startLocalDate.PlusHours(24).ToInstant());
            case CustomEventTiming.ALL_DAY:
                return (startLocalDate.ToInstant(), startLocalDate.PlusHours(24).ToInstant());
            default:
                throw new ArgumentOutOfRangeException(nameof(timing), timing, null);
        }
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
            CustomEventTiming.ACM => new[] { CustomEventTiming.ACM, CustomEventTiming.CM, CustomEventTiming.PCM },
            CustomEventTiming.CM => new[] { CustomEventTiming.ACM, CustomEventTiming.CM, CustomEventTiming.PCM },
            CustomEventTiming.PCM => new[] { CustomEventTiming.ACM, CustomEventTiming.CM, CustomEventTiming.PCM },
            CustomEventTiming.ACD => new[] { CustomEventTiming.ACD, CustomEventTiming.CD, CustomEventTiming.PCD },
            CustomEventTiming.CD => new[] { CustomEventTiming.ACD, CustomEventTiming.CD, CustomEventTiming.PCD },
            CustomEventTiming.PCD => new[] { CustomEventTiming.ACD, CustomEventTiming.CD, CustomEventTiming.PCD },
            CustomEventTiming.ACV => new[] { CustomEventTiming.ACV, CustomEventTiming.CV, CustomEventTiming.PCV },
            CustomEventTiming.CV => new[] { CustomEventTiming.ACV, CustomEventTiming.CV, CustomEventTiming.PCV },
            CustomEventTiming.PCV => new[] { CustomEventTiming.ACV, CustomEventTiming.CV, CustomEventTiming.PCV },
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