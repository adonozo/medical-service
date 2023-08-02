namespace QMUL.DiabetesBackend.Service.Tests.Utils;

using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Hl7.Fhir.Model;
using Model;
using Model.Enums;
using Model.Extensions;
using NodaTime;
using Service.Utils;
using Xunit;
using Duration = Hl7.Fhir.Model.Duration;
using Period = Hl7.Fhir.Model.Period;
using ResourceReference = Model.ResourceReference;

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
        var patient = TestUtils.GetStubInternalPatient();
        var reference = this.GetDummyResource();
        var eventsGenerator = new EventsGenerator(patient, timing, reference);

        // Act
        var events = eventsGenerator.GetEvents().ToList();

        // Assert
        events.Count.Should().Be(14, "Timing period is once every day for 14 days");
        events[0].ScheduledDateTime.Should().Be(new LocalDateTime(2020, 1, 1, 10, 0, 0));
    }

    [Fact]
    public void GetEvents_WhenTimingBoundsIsDurationAndPatientHasRecords_ReturnsHealthEventsOnSetStartDate()
    {
        // Arrange
        var medicationStartDate = new LocalDate(2023, 01, 01);
        var dosageId = Guid.NewGuid().ToString();
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
        var patient = TestUtils.GetStubInternalPatient();
        var reference = this.GetDummyResource();
        reference.EventReferenceId = dosageId;
        var eventsGenerator = new EventsGenerator(patient, timing, reference);

        // Act
        var events = eventsGenerator.GetEvents().ToList();

        // Assert
        events.Count.Should().Be(10, "Timing period is once every day for 10 days");
        events.Should().ContainSingle(@event => @event.ScheduledDateTime == new LocalDateTime(2023, 01, 10, 21, 45, 0));
    }

    [Fact]
    public void GetEvents_WhenTimingIsInvalid_ThrowsException()
    {
        // Arrange
        var timing = new Timing
        {
            Repeat = new Timing.RepeatComponent
            {
                Bounds = new Hl7.Fhir.Model.Range()
            }
        };
        var patient = TestUtils.GetStubInternalPatient();
        var reference = this.GetDummyResource();
        var eventsGenerator = new EventsGenerator(patient, timing, reference);

        // Act
        var action = new Func<IEnumerable<HealthEvent>>(() => eventsGenerator.GetEvents());

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
        var patient = TestUtils.GetStubInternalPatient();
        var reference = this.GetDummyResource();
        var eventsGenerator = new EventsGenerator(patient, timing, reference);

        // Act
        var events = eventsGenerator.GetEvents().ToList();

        // Assert
        events.Count.Should().Be(9, "There are 4 Mondays and 5 Fridays in Jan 2020");
        events[0].ScheduledDateTime.DayOfWeek.Should().Be(DayOfWeek.Friday);
        events[1].ScheduledDateTime.DayOfWeek.Should().Be(DayOfWeek.Monday);
    }

    [Fact]
    public void GetEvents_WhenTimingHasFrequencyGreaterThanOne_ReturnsHealthEvents()
    {
        // Arrange
        var startDate = new LocalDate(2023, 03, 20);
        var dosageId = Guid.NewGuid().ToString();
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

        var patient = TestUtils.GetStubInternalPatient();
        var reference = this.GetDummyResource();
        reference.EventReferenceId = dosageId;
        var eventsGenerator = new EventsGenerator(patient, timing, reference);

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

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void GetEvents_WhenTimingHasCustomTimings_ReturnsHealthEvents(bool setStartDate)
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

        var patient = TestUtils.GetStubInternalPatient();
        patient.ExactEventTimes[CustomEventTiming.ACM] = new LocalTime(08, 00);
        patient.ExactEventTimes[CustomEventTiming.ACV] = new LocalTime(19, 00);
        var reference = this.GetDummyResource();
        if (setStartDate)
        {
            timing.SetStartDate(new LocalDate(2023, 01, 01));
        }

        var eventsGenerator = new EventsGenerator(patient, timing, reference);

        // Act
        var action = () => eventsGenerator.GetEvents().ToList();

        // Assert
        if (setStartDate)
        {
            var events = action();
            events.Count.Should().Be(20, "This is a daily event happening twice a day (ACM and ACV) for 10 days");
            events[0].ScheduledDateTime.Hour.Should().Be(8);
            events[1].ScheduledDateTime.Hour.Should().Be(19);
        }
        else
        {
            action.Should().Throw<InvalidOperationException>();
        }
    }

    [Fact]
    public void GetEvents_WhenTimingHasCustomTimingsButPatientDoesnt_ThrowsException()
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

        var patient = TestUtils.GetStubInternalPatient();
        var reference = this.GetDummyResource();
        reference.StartDate = new LocalDate(2023, 01, 01);
        var eventsGenerator = new EventsGenerator(patient, timing, reference);

        // Act
        var action = () => eventsGenerator.GetEvents().ToList();

        // Assert
        action.Should().Throw<InvalidOperationException>();
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
        var patient = TestUtils.GetStubInternalPatient();
        var reference = this.GetDummyResource();
        var eventsGenerator = new EventsGenerator(patient, timing, reference);

        // Act
        var action = new Func<IEnumerable<HealthEvent>>(() => eventsGenerator.GetEvents());

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
                Period = 10, // Causes an exception
                Frequency = 1,
                Bounds = new Period
                {
                    Start = "2020-01-01",
                    End = "2020-01-10"
                }
            }
        };
        var patient = TestUtils.GetStubInternalPatient();
        var reference = this.GetDummyResource();
        var eventsGenerator = new EventsGenerator(patient, timing, reference);

        // Act
        var action = new Func<IEnumerable<HealthEvent>>(() => eventsGenerator.GetEvents());

        // Assert
        action.Should().Throw<InvalidOperationException>();
    }

    #region Private Methods

    private ResourceReference GetDummyResource()
    {
        return new ResourceReference
        {
            DomainResourceId = string.Empty,
            EventReferenceId = string.Empty,
            EventType = EventType.MedicationDosage
        };
    }

    #endregion
}