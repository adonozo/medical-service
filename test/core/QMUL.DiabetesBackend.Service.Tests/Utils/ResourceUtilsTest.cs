namespace QMUL.DiabetesBackend.Service.Tests.Utils;

using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Hl7.Fhir.Model;
using Model;
using Model.Enums;
using Service.Utils;
using Xunit;

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

    [Theory]
    [InlineData(AlexaRequestType.Medication, EventType.MedicationDosage)]
    [InlineData(AlexaRequestType.Insulin, EventType.InsulinDosage)]
    [InlineData(AlexaRequestType.Glucose, EventType.Measurement)]
    public void MapRequestToEventType_WhenEquivalenceExists_ReturnsEventType(AlexaRequestType alexaType,
        EventType expectedType)
    {
        // Arrange and Act
        var result = ResourceUtils.MapRequestToEventType(alexaType);

        // Assert
        result.Should().Be(expectedType);
    }

    [Fact]
    public void MapRequestToEventType_WhenEquivalenceDoesNotExists_ThrowsException()
    {
        // Arrange and Act
        var action = new Func<EventType>(() => ResourceUtils.MapRequestToEventType(AlexaRequestType.Appointment));

        // Assert
        action.Should().Throw<ArgumentOutOfRangeException>();
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
        events[0].ResourceReference.EventType.Should().Be(EventType.Measurement);
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
        var action = new Func<IEnumerable<HealthEvent>>(() =>
            ResourceUtils.GenerateEventsFrom(serviceRequest, patient));

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
        events[0].ResourceReference.EventType.Should().Be(EventType.MedicationDosage);
    }
}