namespace QMUL.DiabetesBackend.ServiceImpl.Tests.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using FluentAssertions;
    using Hl7.Fhir.Model;
    using Model;
    using Model.Enums;
    using ServiceImpl.Utils;
    using Xunit;
    using ResourceReference = Model.ResourceReference;

    public class EventsGeneratorTest
    {
        [Fact]
        public void GetEvents_WhenTimingBoundsIsPeriod_ReturnsHealthEvents()
        {
            // Arrange
            var startDate = new DateTime(2020, 1, 1, 10, 0 ,0).ToString("O");
            var endDate = new DateTime(2020, 1, 14, 10, 0 ,0).ToString("O");
            var timing = new Timing
            {
                Repeat = new Timing.RepeatComponent
                {
                    PeriodUnit = Timing.UnitsOfTime.D,
                    Period = 1,
                    Frequency = 1,
                    TimeOfDay = new [] { "10:00" },
                    Bounds = new Period
                    {
                        Start = startDate, 
                        End = endDate
                    }
                },
            };
            var patient = this.GetDummyPatient();
            var reference = this.GetDummyResource();
            var eventsGenerator = new EventsGenerator(patient, timing, reference);

            // Act
            var events = eventsGenerator.GetEvents().ToList();

            // Assert
            events.Count.Should().Be(14, "Timing period is once every day for 14 days");
            events[0].EventDateTime.Should().Be(new DateTime(2020, 1, 1, 10, 0, 0));
        }

        [Fact]
        public void GetEvents_WhenTimingBoundsIsDurationAndPatientHasRecords_ReturnsHealthEventsOnSetStartDate()
        {
            // Arrange
            var medicationStartDateTime = new DateTime(2020, 1, 1, 10, 0, 0);
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
                    TimeOfDay = new [] { "11:00" }
                }
            };
            var patient = this.GetDummyPatient();
            patient.ResourceStartDate[dosageId] = medicationStartDateTime;
            var reference = this.GetDummyResource();
            reference.EventReferenceId = dosageId;
            var eventsGenerator = new EventsGenerator(patient, timing, reference);

            // Act
            var events = eventsGenerator.GetEvents().ToList();

            // Assert
            events.Count.Should().Be(10, "Timing period is once every day for 10 days");
            events[0].EventDateTime.Should().Be(new DateTime(2020, 1, 1, 11, 0, 0));
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
            var patient = this.GetDummyPatient();
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
            var startDate = new DateTime(2020, 1, 1, 10, 0 ,0).ToString("O");
            var endDate = new DateTime(2020, 2, 1, 10, 0 ,0).ToString("O");
            var timing = new Timing
            {
                Repeat = new Timing.RepeatComponent
                {
                    PeriodUnit = Timing.UnitsOfTime.D,
                    Period = 1,
                    Frequency = 1,
                    TimeOfDay = new [] { "10:00" },
                    Bounds = new Period
                    {
                        Start = startDate, 
                        End = endDate
                    },
                    DayOfWeek = new DaysOfWeek?[]
                    {
                        DaysOfWeek.Mon, DaysOfWeek.Fri
                    }
                }
            };
            var patient = GetDummyPatient();
            var reference = this.GetDummyResource();
            var eventsGenerator = new EventsGenerator(patient, timing, reference);

            // Act
            var events = eventsGenerator.GetEvents().ToList();

            // Assert
            events.Count.Should().Be(9, "There are 4 Mondays and 5 Fridays in Jan 2020");
            events[0].EventDateTime.DayOfWeek.Should().Be(DayOfWeek.Friday);
            events[1].EventDateTime.DayOfWeek.Should().Be(DayOfWeek.Monday);
        }

        [Fact]
        public void GetEvents_WhenTimingHasFrequencyGreaterThanOne_ReturnsHealthEvents()
        {
            // Arrange
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
            var patient = this.GetDummyPatient();
            patient.ResourceStartDate[dosageId] = new DateTime(2020, 1, 1, 10, 0, 0);
            var reference = this.GetDummyResource();
            reference.EventReferenceId = dosageId;
            var eventsGenerator = new EventsGenerator(patient, timing, reference);

            // Act
            var events = eventsGenerator.GetEvents().ToList();

            // Assert
            events.Count.Should().Be(20, "Timing is twice a day (frequency = 2, period = 1) for 10 days");
            events[0].EventDateTime.Hour.Should().Be(10);
            events[1].EventDateTime.Hour.Should().Be(22);
        }

        [Fact]
        public void GetEvents_WhenTimingHasCustomTimings_ReturnsHealthEvents()
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

            var patient = this.GetDummyPatient();
            patient.ExactEventTimes[CustomEventTiming.ACM] = new DateTime(2020, 1 , 1, 8, 0, 0);
            patient.ExactEventTimes[CustomEventTiming.ACV] = new DateTime(2020, 1 , 1, 19, 0, 0);
            var reference = this.GetDummyResource();
            var eventsGenerator = new EventsGenerator(patient, timing, reference);
            
            // Act
            var events = eventsGenerator.GetEvents().ToList();

            // Assert
            events.Count.Should().Be(20, "This is a daily event happening twice a day (ACM and ACV) for 10 days");
            events[0].EventDateTime.Hour.Should().Be(8);
            events[1].EventDateTime.Hour.Should().Be(19);
        }

        [Fact]
        public void GetEvents_WhenTimingHasCustomTimingsButPatientDoesnt_ReturnsHealthEventsWithoutDefinedHours()
        {
            // Arrange
            var serviceId = Guid.NewGuid().ToString();
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

            var patient = this.GetDummyPatient();
            patient.ResourceStartDate[serviceId] = new DateTime(2020, 1, 1, 10, 0, 0);
            var reference = this.GetDummyResource();
            var eventsGenerator = new EventsGenerator(patient, timing, reference);
            
            // Act
            var events = eventsGenerator.GetEvents().ToList();

            // Assert
            events.Count.Should().Be(20, "This is a daily event happening twice a day (ACM and ACV) for 10 days");
            events[0].EventDateTime.Hour.Should().Be(0);
            events[1].EventDateTime.Hour.Should().Be(0, "All instances have hour 0 when the patient doesn't have a time for the timing event");
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
                    Frequency = 1,
                    Bounds = new Period
                    {
                        Start = new DateTime(2020, 1, 1).ToString("O"), 
                        End = new DateTime(2020, 1, 10).ToString("O")
                    }
                }
            };
            var patient = this.GetDummyPatient();
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
                        Start = new DateTime(2020, 1, 1).ToString("O"), 
                        End = new DateTime(2020, 1, 10).ToString("O")
                    }
                }
            };
            var patient = this.GetDummyPatient();
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
                ResourceId = string.Empty,
                EventReferenceId = string.Empty,
                EventType = EventType.MedicationDosage
            };
        }
        
        private InternalPatient GetDummyPatient()
        {
            return new InternalPatient
            {
                Id = Guid.NewGuid().ToString(), 
                ExactEventTimes = new Dictionary<CustomEventTiming, DateTimeOffset>(),
                ResourceStartDate = new Dictionary<string, DateTime>()
            };
        }
        
        #endregion
    }
}