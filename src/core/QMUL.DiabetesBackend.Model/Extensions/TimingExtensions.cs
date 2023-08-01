namespace QMUL.DiabetesBackend.Model.Extensions;

using System;
using System.Collections.Generic;
using System.Globalization;
using Constants;
using Hl7.Fhir.Model;
using NodaTime;
using NodaTime.Text;
using Duration = Hl7.Fhir.Model.Duration;
using Period = Hl7.Fhir.Model.Period;

public static class TimingExtensions
{
    /// <summary>
    /// Gets the patient's start for a resource that has a <see cref="Timing"/> instance. This date is stored as an
    /// extension value.
    /// </summary>
    /// <param name="timing">The <see cref="Timing"/> from the resource</param>
    /// <returns>The <see cref="LocalDate"/> start date, or null if the value was not found.</returns>
    public static LocalDate? GetStartDate(this Timing timing)
    {
        var extension = timing.GetExtension(Extensions.TimingStartDate);
        if (extension?.Value is not FhirString startDateString)
        {
            return null;
        }

        var parseResult = LocalDatePattern.Iso.Parse(startDateString.Value);
        return parseResult.TryGetValue(default, out var localDate) ? localDate : null;
    }

    /// <summary>
    /// Holds the exact date for a resource to start. Should be used when the resource has a frequency rather than a
    /// period. For example, a medication that must be taken for 14 days.
    /// </summary>
    /// <param name="timing">The medication service's occurrence timing.</param>
    /// <param name="date">The date when this dosage has/will start</param>
    public static void SetStartDate(this Timing timing, LocalDate date)
    {
        var fhirString = new FhirString(date.ToString("R", CultureInfo.InvariantCulture));
        timing.SetExtension(Extensions.TimingStartDate, fhirString);
    }

    /// <summary>
    /// Adds an extension to a <see cref="Timing"/> to indicate that the resource needs a start date
    /// </summary>
    /// <param name="timing">The <see cref="Timing"/> to add the extension to</param>
    public static void SetNeedsStartDateFlag(this Timing timing)
    {
        timing.SetExtension(Extensions.NeedsStartDateFlag, new FhirBoolean(true));
    }

    /// <summary>
    /// Removes the flag extension from the <see cref="Timing"/>. To use when the resource has start date
    /// </summary>
    /// <param name="timing">The <see cref="Timing"/> to remove the extension from</param>
    public static void RemoveNeedsStartDateFlag(this Timing timing)
    {
        timing.RemoveExtension(Extensions.NeedsStartDateFlag);
    }

    /// <summary>
    /// Checks if the <see cref="Timing.RepeatComponent"/> needs a start date. This is always true for <see cref="Hl7.Fhir.Model.Duration"/>
    /// repeat components as it does not have an start date by design (e.g., a duration of 3 weeks - no start date).
    /// </summary>
    /// <param name="repeat">The resource <see cref="Timing.RepeatComponent"/></param>
    /// <returns>A boolean value to indicate if the resource needs a start date</returns>
    /// <exception cref="ArgumentException">If the repeat component is not <see cref="Hl7.Fhir.Model.Period"/> or <see cref="Hl7.Fhir.Model.Duration"/></exception>
    public static bool NeedsStartDate(this Timing.RepeatComponent repeat) => repeat.Bounds switch
    {
        Period bounds => string.IsNullOrEmpty(bounds.Start),
        Duration => true,
        _ => throw new ArgumentException("Repeat component has not a valid bound", nameof(repeat))
    };

    /// <summary>
    /// Gets the start time as <see cref="LocalTime"/> from a resource's timing
    /// </summary>
    /// <param name="timing">The resource timing</param>
    /// <returns>The resource start time as <see cref="LocalTime"/></returns>
    public static LocalTime? GetStartTime(this Timing timing)
    {
        var extension = timing.GetExtension(Extensions.TimingStartTime);
        if (extension?.Value is not FhirString startDateString)
        {
            return null;
        }

        var parseResult = LocalTimePattern.GeneralIso.Parse(startDateString.Value);
        return parseResult.Success ? parseResult.Value : null;
    }

    /// <summary>
    /// Sets the start time in a resource's timing
    /// </summary>
    /// <param name="timing">The resource <see cref="Timing"/></param>
    /// <param name="time">The defined <see cref="LocalTime"/></param>
    public static void SetStartTime(this Timing timing, LocalTime time)
    {
        var fhirString = new FhirString(time.ToString("T", CultureInfo.InvariantCulture));
        timing.SetExtension(Extensions.TimingStartTime, fhirString);
    }

    /// <summary>
    /// Adds a flag to tell if the timing needs a start time
    /// </summary>
    /// <param name="timing">The resource's timing</param>
    public static void SetNeedsStartTimeFlag(this Timing timing)
    {
        timing.SetExtension(Extensions.NeedsStartTimeFlag, new FhirBoolean(true));
    }

    /// <summary>
    /// Removes the flag that tells if the timing needs a start time
    /// </summary>
    /// <param name="timing">The resource's timing</param>
    public static void RemoveNeedsStartTimeFlag(this Timing timing)
    {
        timing.RemoveExtension(Extensions.NeedsStartTimeFlag);
    }

    /// <summary>
    /// Checks if a timing needs a start time. This is true if the bounds in the repeat component is of type <see cref="Period"/>
    /// and the frequency is greater than 1, which means that the event is repeated multiple times in the day
    /// </summary>
    /// <param name="repeat">The timing's repeat component</param>
    /// <returns>True if the timing would need a start time</returns>
    /// <exception cref="InvalidOperationException">When the bounds are of type <see cref="Period"/> and the period unit
    /// is not 'day', as there is no support for other units of time yet</exception>
    public static bool NeedsStartTime(this Timing.RepeatComponent repeat) => repeat switch
    {
        { Bounds: Period, PeriodUnit: Timing.UnitsOfTime.D } => repeat.Frequency is > 1,
        { Bounds: Period, PeriodUnit: not Timing.UnitsOfTime.D } =>
            throw new InvalidOperationException($"Unsupported PeriodUnit: {repeat.PeriodUnit}"),
        _ => false
    };

    /// <summary>
    /// Gets the start date and end dates from a <see cref="Period"/>
    /// </summary>
    /// <param name="period">The timing <see cref="Period"/></param>
    /// <returns>A tuple of <see cref="LocalDate"/> with the start and end dates</returns>
    public static (LocalDate StartDate, LocalDate EndDate) GetDatesFromPeriod(this Period period)
    {
        var pattern = LocalDatePattern.Iso;
        var startDate = pattern.Parse(period.Start[..10]);
        var endDate = pattern.Parse(period.End[..10]);
        return (startDate.GetValueOrThrow(), endDate.GetValueOrThrow());
    }

    /// <summary>
    /// Iterates the TimeOfDay values of a <see cref="Timing.RepeatComponent"/> as ISO time strings by adding the seconds
    /// </summary>
    /// <param name="repeat">The <see cref="Timing.RepeatComponent"/> contained in a resource's Timing</param>
    /// <returns>An iterable list of ISO string times</returns>
    public static IEnumerable<string> TimeOfDayIso(this Timing.RepeatComponent repeat)
    {
        repeat.TimeOfDay ??= Array.Empty<string>();
        foreach (var timeOfDay in repeat.TimeOfDay)
        {
            yield return timeOfDay + ":00";
        }
    }
}