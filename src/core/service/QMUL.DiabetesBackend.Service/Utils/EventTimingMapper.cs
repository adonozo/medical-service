namespace QMUL.DiabetesBackend.Service.Utils;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Hl7.Fhir.Model;
using Model.Enums;
using NodaTime;
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
    /// <param name="patientTimingPreferences">The patient's timing preferences in a <see cref="Dictionary{TKey,TValue}"/>.</param>
    /// <param name="localDate">The reference start date for the interval.</param>
    /// <param name="timing">The timing event.</param>
    /// <param name="timezone">The patient's timezone.</param>
    /// <param name="defaultOffset">An offset for the time interval, in minutes.</param>
    /// <returns>An <see cref="Instant"/> Tuple with the start and end datetime.</returns>
    public static Interval TimingIntervalForPatient(
        Dictionary<CustomEventTiming, LocalTime> patientTimingPreferences,
        LocalDate localDate,
        CustomEventTiming timing,
        string timezone,
        int defaultOffset)
    {
        if (!patientTimingPreferences.ContainsKey(timing))
        {
            return GetDefaultIntervalFromEventTiming(localDate, timing, timezone);
        }

        var start = localDate.At(patientTimingPreferences[timing]).Plus(Period.FromMinutes(defaultOffset * -1));
        var end = localDate.At(patientTimingPreferences[timing]).Plus(Period.FromMinutes(defaultOffset));
        return new Interval(start.InUtc().ToInstant(), end.InUtc().ToInstant());
    }

    /// <summary>
    /// Gets the default interval time for a Timing event, considering the patient's timezone.
    /// </summary>
    /// <param name="localDate">The reference start date for the interval.</param>
    /// <param name="timing">The timing event.</param>
    /// <param name="timezone">The patient's timezone.</param>
    /// <returns>An <see cref="Instant"/> Tuple with the start and end datetime.</returns>
    /// <exception cref="ArgumentOutOfRangeException">If there is not a default value for the timing event.</exception>
    public static Interval GetDefaultIntervalFromEventTiming(LocalDate localDate,
        CustomEventTiming timing,
        string timezone)
    {
        var zone = DateTimeZoneProviders.Tzdb[timezone];
        var startLocalDate = localDate.AtStartOfDayInZone(zone);

        switch (timing)
        {
            case CustomEventTiming.MORN:
                return new Interval(startLocalDate.PlusHours(6).ToInstant(), startLocalDate.PlusHours(12).ToInstant());
            case CustomEventTiming.MORN_early:
                return new Interval(startLocalDate.PlusHours(6).ToInstant(), startLocalDate.PlusHours(9).ToInstant());
            case CustomEventTiming.MORN_late:
                return new Interval(startLocalDate.PlusHours(9).ToInstant(), startLocalDate.PlusHours(12).ToInstant());
            case CustomEventTiming.NOON:
                return new Interval(startLocalDate.PlusHours(11).PlusMinutes(30).ToInstant(),
                    startLocalDate.PlusHours(12).PlusMinutes(30).ToInstant());
            case CustomEventTiming.AFT:
                return new Interval(startLocalDate.PlusHours(12).ToInstant(), startLocalDate.PlusHours(18).ToInstant());
            case CustomEventTiming.AFT_early:
                return new Interval(startLocalDate.PlusHours(12).ToInstant(), startLocalDate.PlusHours(15).ToInstant());
            case CustomEventTiming.AFT_late:
                return new Interval(startLocalDate.PlusHours(15).ToInstant(), startLocalDate.PlusHours(18).ToInstant());
            case CustomEventTiming.EVE:
                return new Interval(startLocalDate.PlusHours(18).ToInstant(), startLocalDate.PlusHours(24).ToInstant());
            case CustomEventTiming.EVE_early:
                return new Interval(startLocalDate.PlusHours(18).ToInstant(), startLocalDate.PlusHours(21).ToInstant());
            case CustomEventTiming.EVE_late:
                return new Interval(startLocalDate.PlusHours(21).ToInstant(), startLocalDate.PlusHours(24).ToInstant());
            case CustomEventTiming.NIGHT:
                return new Interval(startLocalDate.PlusHours(18).ToInstant(), startLocalDate.PlusHours(27).ToInstant());
            case CustomEventTiming.ACM:
            case CustomEventTiming.CM:
            case CustomEventTiming.PCM:
                return new Interval(startLocalDate.PlusHours(6).ToInstant(), startLocalDate.PlusHours(11).ToInstant());
            case CustomEventTiming.ACD:
            case CustomEventTiming.CD:
            case CustomEventTiming.PCD:
                return new Interval(startLocalDate.PlusHours(11).ToInstant(), startLocalDate.PlusHours(17).ToInstant());
            case CustomEventTiming.ACV:
            case CustomEventTiming.CV:
            case CustomEventTiming.PCV:
                return new Interval(startLocalDate.PlusHours(17).ToInstant(), startLocalDate.PlusHours(24).ToInstant());
            case CustomEventTiming.ALL_DAY:
                return new Interval(startLocalDate.ToInstant(), startLocalDate.PlusHours(24).ToInstant());
            default:
                throw new ArgumentOutOfRangeException(nameof(timing), timing, null);
        }
    }
}