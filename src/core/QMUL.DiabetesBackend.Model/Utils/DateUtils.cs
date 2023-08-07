namespace QMUL.DiabetesBackend.Model.Utils;

using NodaTime;

public static class DateUtils
{
    /// <summary>
    /// Converts a UTC date into an <see cref="Instant"/>
    /// </summary>
    /// <param name="date">The date to convert</param>
    /// <returns>An <see cref="Instant"/> at the start of day (00:00:00)</returns>
    public static Instant InstantFromUtcDate(LocalDate date) => date.AtStartOfDayInZone(DateTimeZone.Utc).ToInstant();

    /// <summary>
    /// Construct an Instant from a date and time
    /// </summary>
    /// <param name="date">A UTC date</param>
    /// <param name="time">The time of the day</param>
    /// <returns>An <see cref="Instant"/></returns>
    public static Instant InstantFromUtcDateAndTime(LocalDate date, LocalTime time) =>
        date.At(time).InZoneStrictly(DateTimeZone.Utc).ToInstant();

    /// <summary>
    /// Construct an Instant from a UTC datetime
    /// </summary>
    /// <param name="dateTime">A UTC datetime</param>
    /// <returns>An <see cref="Instant"/></returns>
    public static Instant InstantFromUtcDateTime(LocalDateTime dateTime) =>
        dateTime.InZoneStrictly(DateTimeZone.Utc).ToInstant();
}