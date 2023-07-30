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

    [Fact(Skip = "Events generator should use a LocalDateTime to support frequencies > 1")]
    public void GetEvents_WhenTimingHasFrequencyGreaterThanOne_ReturnsHealthEvents()
    {
        // Arrange
        var startDate = new LocalDate(2023, 01, 01);
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
        var patient = TestUtils.GetStubInternalPatient();
        var reference = this.GetDummyResource();
        reference.EventReferenceId = dosageId;
        reference.StartDate = startDate;
        var eventsGenerator = new EventsGenerator(patient, timing, reference);

        // Act
        var events = eventsGenerator.GetEvents().ToList();

        // Assert
        events.Count.Should().Be(20, "Timing is twice a day (frequency = 2, period = 1) for 10 days");
        events[0].ScheduledDateTime.Hour.Should().Be(10);
        events[1].ScheduledDateTime.Hour.Should().Be(22);
    }

    [Theory(Skip = "If start date is not set, the method will throw an exception")]
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
            reference.StartDate = new LocalDate(2023, 01, 01);
        }

        var eventsGenerator = new EventsGenerator(patient, timing, reference);

        // Act
        var events = eventsGenerator.GetEvents().ToList();

        // Assert
        if (setStartDate)
        {
            events.Count.Should().Be(20, "This is a daily event happening twice a day (ACM and ACV) for 10 days");
            events[0].ScheduledDateTime.Hour.Should().Be(8);
            events[1].ScheduledDateTime.Hour.Should().Be(19);
        }
        else
        {
            events.Count.Should().Be(0, "Don't create events if there is no start date");
        }
    }

    [Fact(Skip = "Method will throw when patient doesn't have a time for the timing event")]
    public void GetEvents_WhenTimingHasCustomTimingsButPatientDoesnt_ReturnsHealthEventsWithoutDefinedHours()
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
        var events = eventsGenerator.GetEvents().ToList();

        // Assert
        events.Count.Should().Be(20, "This is a daily event happening twice a day (ACM and ACV) for 10 days");
        events[0].ScheduledDateTime.Hour.Should().Be(0);
        events[1].ScheduledDateTime.Hour.Should().Be(0,
            "All instances have hour 0 when the patient doesn't have a time for the timing event");
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