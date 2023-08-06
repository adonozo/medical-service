namespace QMUL.DiabetesBackend.Service.Tests.Utils;

using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Hl7.Fhir.Model;
using Model.Enums;
using NodaTime;
using Service.Utils;
using Xunit;
using Period = Hl7.Fhir.Model.Period;

public class ResourceUtilsTest
{
    [Fact]
    public void GenerateSearchBundle_ReturnsABundleInstance()
    {
        // Arrange
        var entries = new List<Resource> { TestUtils.GetStubPatient() };

        // Arrange and Act
        var bundle = ResourceUtils.GenerateSearchBundle(entries);

        // Assert
        bundle.Should().BeOfType<Bundle>();
    }

    [Fact]
    public void GenerateSearchBundle_ReturnsBundleTypeAndCurrentDate()
    {
        // Arrange
        var currentDate = DateTime.UtcNow.ToString("d");

        // Act
        var bundle = ResourceUtils.GenerateSearchBundle(new List<Resource>());

        // Assert
        bundle.Type.Should().Be(Bundle.BundleType.Searchset);
        // ReSharper disable once PossibleInvalidOperationException
        bundle.Timestamp.Value.Date.Date.ToString("d").Should().Be(currentDate);
    }

    [Fact]
    public void GenerateEventsFrom_WhenIsServiceRequest_ReturnsHealthEvents()
    {
        // Arrange
        var patient = TestUtils.GetStubInternalPatient();
        var timing = new Timing
        {
            Repeat = new Timing.RepeatComponent
            {
                PeriodUnit = Timing.UnitsOfTime.D,
                Period = 1,
                Frequency = 1,
                Bounds = new Period
                {
                    Start = "2023-01-01",
                    End = "2023-01-10"
                },
                TimeOfDay = new[] { "10:00" }
            }
        };
        var contained = new ServiceRequest
        {
            Id = Guid.NewGuid().ToString(),
            Occurrence = timing
        };

        var serviceRequest = new ServiceRequest
        {
            Id = Guid.NewGuid().ToString(),
            Contained = new List<Resource> { contained },
            Occurrence = timing
        };

        // Act
        var events = ResourceUtils.GenerateEventsFrom(serviceRequest, patient).ToList();

        // Assert
        events.Count.Should().Be(10);
        events.MinBy(@event => @event.ScheduledDateTime).ScheduledDateTime.Should().BeEquivalentTo(new LocalDateTime(2023, 01, 01, 10, 00));
        events.MaxBy(@event => @event.ScheduledDateTime).ScheduledDateTime.Should().BeEquivalentTo(new LocalDateTime(2023, 01, 10, 10, 00));
    }

    [Fact]
    public void GenerateEventsFrom_WhenIsServiceRequestWithoutTiming_ThrowsException()
    {
        // Arrange
        var patient = TestUtils.GetStubInternalPatient();
        var contained = new ServiceRequest
        {
            Id = Guid.NewGuid().ToString(),
            Occurrence = new Period
            {
                Start = new DateTime(2020, 1, 1).ToString("O"),
                End = new DateTime(2020, 1, 14).ToString("O")
            },
        };
        
        var serviceRequest = new ServiceRequest
        {
            Id = Guid.NewGuid().ToString(),
            Contained = new List<Resource> { contained }
        };

        // Act
        var action = () => ResourceUtils.GenerateEventsFrom(serviceRequest, patient);

        // Assert
        action.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void GenerateEventsFrom_WhenIsMedicationRequest_ReturnsHealthEvents()
    {
        // Arrange
        var patient = TestUtils.GetStubInternalPatient();
        var timing = new Timing
        {
            Repeat = new Timing.RepeatComponent
            {
                PeriodUnit = Timing.UnitsOfTime.D,
                Period = 1,
                Frequency = 1,
                Bounds = new Period
                {
                    Start = "2023-01-01",
                    End = "2023-01-10"
                },
                TimeOfDay = new[] { "10:00" }
            }
        };
        var medicationRequest = new MedicationRequest
        {
            DosageInstruction = new List<Dosage>
            {
                new()
                {
                    ElementId = Guid.NewGuid().ToString(),
                    Timing = timing
                }
            }
        };

        // Act
        var events = ResourceUtils.GenerateEventsFrom(medicationRequest, patient).ToList();

        // Assert
        events.Count.Should().Be(10);
        events.MinBy(@event => @event.ScheduledDateTime).ScheduledDateTime.Should().BeEquivalentTo(new LocalDateTime(2023, 01, 01, 10, 00));
        events.MaxBy(@event => @event.ScheduledDateTime).ScheduledDateTime.Should().BeEquivalentTo(new LocalDateTime(2023, 01, 10, 10, 00));
    }
}