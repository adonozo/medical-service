namespace QMUL.DiabetesBackend.ServiceImpl.Tests.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Hl7.Fhir.Model;
    using Model;
    using Model.Enums;
    using ServiceImpl.Utils;
    using Xunit;
    using Patient = Model.Patient;
    using Task = System.Threading.Tasks.Task;

    public class ResourceUtilsTest
    {
        [Fact]
        public void GenerateEmptyBundle_ReturnsABundleInstance()
        {
            // Arrange and Act
            var bundle = ResourceUtils.GenerateEmptyBundle();

            // Assert
            bundle.Should().BeOfType<Bundle>();
        }

        [Fact]
        public void GenerateEmptyBundle_ReturnsBundleTypeAndCurrentDate()
        {
            // Arrange
            var currentDate = DateTime.UtcNow.ToString("d");
            
            // Act
            var bundle = ResourceUtils.GenerateEmptyBundle();
            
            // Assert
            bundle.Type.Should().Be(Bundle.BundleType.Searchset);
            // ReSharper disable once PossibleInvalidOperationException
            bundle.Timestamp.Value.Date.Date.ToString("d").Should().Be(currentDate);
        }

        [Fact]
        public void IsInsulinResource_WhenExtensionContainsInsulin_ReturnsTrue()
        {
            // Arrange
            var medicationRequest = new MedicationRequest
            {
                Extension = new List<Extension>
                {
                    new() { Url = "http://localhost/type/insulin"}
                }
            };
            
            // Act
            var isInsulin = ResourceUtils.IsInsulinResource(medicationRequest);
            
            // Assert
            isInsulin.Should().Be(true);
        }

        [Fact]
        public void IsInsulinResource_WhenDoesNotHaveExtension_ReturnsFalse()
        {
            // Arrange
            var medicationRequest = new MedicationRequest
            {
                Id = Guid.NewGuid().ToString()
            };

            // Act
            var isInsulin = ResourceUtils.IsInsulinResource(medicationRequest);

            // Assert
            isInsulin.Should().Be(false);
        }

        [Fact]
        public async Task ValidateNullObject_WhenResultIsNotNull_ReturnsObject()
        {
            // Arrange
            var function = new Func<Task<string>>(() => Task.FromResult(string.Empty));
            var exception = new Exception();

            // Act
            var result = await ResourceUtils.ValidateNullObject(function, exception);

            // Assert
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task ValidateNullObject_WhenResultIsNull_ThrowsException()
        {
            // Arrange
            var function = new Func<Task<string>>(() => Task.FromResult<string>(null));
            var exception = new ArgumentException();
            
            // Act
            var action =
                new Func<Task<string>>(() => ResourceUtils.ValidateNullObject(function, exception));
            
            // Assert
            await action.Should().ThrowAsync<ArgumentException>();
        }

        [Theory]
        [InlineData(AlexaRequestType.Medication, EventType.MedicationDosage)]
        [InlineData(AlexaRequestType.Insulin, EventType.InsulinDosage)]
        [InlineData(AlexaRequestType.Glucose, EventType.Measurement)]
        public void MapRequestToEventType_WhenEquivalenceExists_ReturnsEventType(AlexaRequestType alexaType, EventType expectedType)
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
            var patient = this.GetDummyPatient();
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
                    TimeOfDay = new [] { "10:00" }
                }
            };
            var serviceRequest = new ServiceRequest
            {
                Id = Guid.NewGuid().ToString(),
                Occurrence = timing
            };
        
            // Act
            var events = ResourceUtils.GenerateEventsFrom(serviceRequest, patient).ToList();
        
            // Assert
            events.Count.Should().Be(10);
            events[0].Resource.EventType.Should().Be(EventType.Measurement);
        }

        [Fact]
        public void GenerateEventsFrom_WhenIsServiceRequestWithoutTiming_ThrowsException()
        {
            // Arrange
            var patient = this.GetDummyPatient();
            var serviceRequest = new ServiceRequest
            {
                Id = Guid.NewGuid().ToString(),
                Occurrence = new Period
                {
                    Start = new DateTime(2020, 1, 1).ToString("O"),
                    End = new DateTime(2020, 1, 14).ToString("O")
                } 
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
            var patient = this.GetDummyPatient();
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
                    TimeOfDay = new [] { "10:00" }
                }
            };
            var medicationRequest = new MedicationRequest
            {
                DosageInstruction = new List<Dosage> { new()
                {
                    ElementId = Guid.NewGuid().ToString(),
                    Timing = timing
                } }
            };
            
            // Act
            var events = ResourceUtils.GenerateEventsFrom(medicationRequest, patient).ToList();
            
            // Assert
            events.Count.Should().Be(10);
            events[0].Resource.EventType.Should().Be(EventType.MedicationDosage);
        }

        #region Private Methods

        private Patient GetDummyPatient()
        {
            return new Patient
            {
                Id = Guid.NewGuid().ToString(), 
                ExactEventTimes = new Dictionary<CustomEventTiming, DateTime>(),
                ResourceStartDate = new Dictionary<string, DateTime>()
            };
        }

        #endregion
    }
}