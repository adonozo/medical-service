namespace QMUL.DiabetesBackend.Service.Tests.Utils;

using System;
using System.Linq;
using FluentAssertions;
using Hl7.Fhir.Model;
using Model.Enums;
using Model.Extensions;
using Model.Utils;
using NodaTime;
using Service.Utils;
using Xunit;
using Duration = Hl7.Fhir.Model.Duration;
using Period = Hl7.Fhir.Model.Period;

public class EventsGeneratorTest
{
    [Fact]
    public void GetEvents_WhenTimingBoundsIsPeriod_ReturnsHealthEvents()
    {
        // Arrange
        var timing = new Timing
        {
            Repeat = new Timing.RepeatComponent
            {
                PeriodUnit = Timing.UnitsOfTime.D,
                Period = 1,
                Frequency = 1,
                TimeOfDay = new[] { "10:00" },
                Bounds = new Period
                {
                    Start = "2020-01-01",
                    End = "2020-01-14"
                }
            },
        };
        var eventsGenerator = new EventsGenerator(timing);

        // Act
        var events = eventsGenerator.GetEvents().ToList();

        // Assert
        events.Count.Should().Be(14, "Timing period is once every day for 14 days");
        events[0].ScheduledDateTime.Should().Be(new LocalDateTime(2020, 1, 1, 10, 0, 0));
    }

    [Fact]
    public void GetEvents_WhenTimingBoundsIsDurationWithStartDate_ReturnsHealthEventsOnSetStartDate()
    {
        // Arrange
        var medicationStartDate = new LocalDate(2023, 01, 01);
        var timing = new Timing
        {
            Repeat = new Timing.RepeatComponent
            {
                PeriodUnit = Timing.UnitsOfTime.D,
                Period = 1,
                Frequency = 1,
                Bounds = new Duration
                {
                    Unit = "d",
                    Value = 10
                },
                TimeOfDay = new[] { "21:45" }
            }
        };
        timing.SetStartDate(medicationStartDate);
        var eventsGenerator = new EventsGenerator(timing);

        // Act
        var events = eventsGenerator.GetEvents().ToList();

        // Assert
        events.Count.Should().Be(10, "Timing period is once every day for 10 days");
        events.Should().ContainSingle(@event => @event.ScheduledDateTime == new LocalDateTime(2023, 01, 10, 21, 45, 0));
    }

    [Fact]
    public void GetEvents_WhenDateFilterIsProvided_ReturnsFilteredHealthEvents()
    {
        // Arrange
        var timing = new Timing
        {
            Repeat = new Timing.RepeatComponent
            {
                PeriodUnit = Timing.UnitsOfTime.D,
                Period = 1,
                Frequency = 1,
                TimeOfDay = new[] { "10:00" },
                Bounds = new Period
                {
                    Start = "2020-01-01",
                    End = "2020-01-14"
                }
            },
        };

        var filterStartDate = new LocalDate(2020, 01, 04);
        var dateFilter = new Interval(
            DateUtils.InstantFromUtcDate(filterStartDate),
            DateUtils.InstantFromUtcDate(filterStartDate.PlusDays(4)));

        var eventsGenerator = new EventsGenerator(timing, dateFilter);

        // Act
        var events = eventsGenerator.GetEvents().ToList();

        // Assert
        events.Count.Should().Be(4, "Timing period is once every day for the provided 4 days filter");
        events[0].ScheduledDateTime.Should().Be(new LocalDateTime(2020, 01, 04, 10, 0, 0));
    }

    [Fact]
    public void GetEvents_WhenTimingBoundsAreInvalid_ThrowsException()
    {
        // Arrange
        var timing = new Timing
        {
            Repeat = new Timing.RepeatComponent
            {
                Bounds = new Hl7.Fhir.Model.Range()
            }
        };

        // Act
        var action = () => new EventsGenerator(timing);

        // Assert
        action.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void GetEvents_WhenTimingHasWeeklyFrequency_ReturnsHealthEvents()
    {
        // Arrange
        var timing = new Timing
        {
            Repeat = new Timing.RepeatComponent
            {
                PeriodUnit = Timing.UnitsOfTime.D,
                Period = 1,
                Frequency = 1,
                TimeOfDay = new[] { "10:00" },
                Bounds = new Period
                {
                    Start = "2020-01-01",
                    End = "2020-02-01"
                },
                DayOfWeek = new DaysOfWeek?[]
                {
                    DaysOfWeek.Mon, DaysOfWeek.Fri
                }
            }
        };
        var eventsGenerator = new EventsGenerator(timing);

        // Act
        var events = eventsGenerator.GetEvents().ToList();

        // Assert
        events.Count.Should().Be(9, "There are 4 Mondays and 5 Fridays in Jan 2020");
        events[0].ScheduledDateTime.DayOfWeek.Should().Be(IsoDayOfWeek.Friday);
        events[1].ScheduledDateTime.DayOfWeek.Should().Be(IsoDayOfWeek.Monday);
    }

    [Fact]
    public void GetEvents_WhenTimingHasFrequencyGreaterThanOne_ReturnsHealthEvents()
    {
        // Arrange
        var startDate = new LocalDate(2023, 03, 20);
        var timing = new Timing
        {
            Repeat = new Timing.RepeatComponent
            {
                PeriodUnit = Timing.UnitsOfTime.D,
                Period = 1,
                Frequency = 2,
                Bounds = new Duration
                {
                    Unit = "d",
                    Value = 10
                }
            }
        };

        timing.SetStartDate(startDate);
        timing.SetStartTime(new LocalTime(10, 00));

        var eventsGenerator = new EventsGenerator(timing);

        // Act
        var events = eventsGenerator.GetEvents().ToList();

        // Assert
        events.Count.Should().Be(20, "Timing is twice a day (frequency = 2, period = 1) for 10 days");
        events[0].ScheduledDateTime.Hour.Should().Be(10);
        events[1].ScheduledDateTime.Hour.Should().Be(22);
        events[18].ScheduledDateTime.Hour.Should().Be(10, "Date is UTC, shouldn't change on summer time");
        events[19].ScheduledDateTime.Hour.Should().Be(22, "Date is UTC, shouldn't change on summer time");
        events.MaxBy(@event => @event.ScheduledDateTime).ScheduledDateTime.Date.Should().Be(new LocalDate(2023, 03, 29));
        events.MinBy(@event => @event.ScheduledDateTime).ScheduledDateTime.Date.Should().Be(new LocalDate(2023, 03, 20));
    }

    [Fact]
    public void GetEvents_WhenTimingHasFrequencyGreaterThanOneWithFilter_ReturnsHealthEvents()
    {
        // Arrange
        var startDate = new LocalDate(2023, 03, 20);
        var timing = new Timing
        {
            Repeat = new Timing.RepeatComponent
            {
                PeriodUnit = Timing.UnitsOfTime.D,
                Period = 1,
                Frequency = 2,
                Bounds = new Duration
                {
                    Unit = "d",
                    Value = 10
                }
            }
        };

        timing.SetStartDate(startDate);
        timing.SetStartTime(new LocalTime(10, 00));

        var dateFilter = this.SameDayInterval(2023, 03, 22);
        var eventsGenerator = new EventsGenerator(timing, dateFilter);

        // Act
        var events = eventsGenerator.GetEvents().ToList();

        // Assert
        events.Count.Should().Be(2, "Timing is twice a day (frequency = 2, period = 1) for 1 days (filtered)");
        events[0].ScheduledDateTime.Hour.Should().Be(10);
        events[1].ScheduledDateTime.Hour.Should().Be(22);
        events.MaxBy(@event => @event.ScheduledDateTime).ScheduledDateTime.Date.Should().Be(new LocalDate(2023, 03, 22));
        events.MinBy(@event => @event.ScheduledDateTime).ScheduledDateTime.Date.Should().Be(new LocalDate(2023, 03, 22));
    }

    [Fact]
    public void GetEvents_WhenTimingHasTimingArrayAndStartDate_ReturnsHealthEvents()
    {
        // Arrange
        var timing = new Timing
        {
            Repeat = new Timing.RepeatComponent
            {
                PeriodUnit = Timing.UnitsOfTime.D,
                Period = 1,
                Frequency = 1,
                Bounds = new Duration
                {
                    Unit = "d",
                    Value = 10
                },
                When = new Timing.EventTiming?[] { Timing.EventTiming.ACM, Timing.EventTiming.ACV }
            }
        };

        timing.SetStartDate(new LocalDate(2023, 01, 01));
        var eventsGenerator = new EventsGenerator(timing);

        // Act
        var events = eventsGenerator.GetEvents().ToList();

        // Assert
        events.Count.Should().Be(20, "This is a daily event happening twice a day (ACM and ACV) for 10 days");
        events[0].EventTiming.Should().Be(CustomEventTiming.ACM);
        events[1].EventTiming.Should().Be(CustomEventTiming.ACV);
    }

    [Fact]
    public void GetEvents_WhenDurationTimingMissesStartDate_ThrowsException()
    {
        // Arrange
        var timing = new Timing
        {
            Repeat = new Timing.RepeatComponent
            {
                PeriodUnit = Timing.UnitsOfTime.D,
                Period = 1,
                Frequency = 1,
                Bounds = new Duration
                {
                    Unit = "d",
                    Value = 10
                },
                When = new Timing.EventTiming?[] { Timing.EventTiming.ACM, Timing.EventTiming.ACV }
            }
        };

        // Act
        var action = () => new EventsGenerator(timing);

        // Assert
        action.Should().Throw<InvalidOperationException>().WithMessage("*start date*");
    }

    [Fact]
    public void GetEvents_WhenTimingDoesNotHaveTimeOfDayOrWhen_ThrowsException()
    {
        // Arrange
        var timing = new Timing
        {
            Repeat = new Timing.RepeatComponent
            {
                PeriodUnit = Timing.UnitsOfTime.D,
                Period = 1,
                Frequency = 1
            }
        };

        // Act
        var action = () => new EventsGenerator(timing);

        // Assert
        action.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void GetEvents_WhenPeriodIsNotOne_ThrowsException()
    {
        // Arrange
        var timing = new Timing
        {
            Repeat = new Timing.RepeatComponent
            {
                PeriodUnit = Timing.UnitsOfTime.D,
                Period = 10, // Unsupported period
                Frequency = 1,
                Bounds = new Period
                {
                    Start = "2020-01-01",
                    End = "2020-01-10"
                }
            }
        };

        var eventsGenerator = new EventsGenerator(timing);

        // Act
        var action = () => eventsGenerator.GetEvents();

        // Assert
        action.Should().Throw<InvalidOperationException>();
    }

    #region Private Methods

    private Interval SameDayInterval(int year, int month, int day)
    {
        var date = new LocalDate(year, month, day);
        var start = DateUtils.InstantFromUtcDate(date);
        var end = DateUtils.InstantFromUtcDate(date.PlusDays(1));
        return new Interval(start, end);
    }

    #endregion
}