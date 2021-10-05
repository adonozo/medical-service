namespace QMUL.DiabetesBackend.ServiceImpl.Tests
{
    using System;
    using FluentAssertions;
    using Hl7.Fhir.Model;
    using Model.Enums;
    using Utils;
    using Xunit;
    using static System.Enum;

    public class EventTimingMapperTest
    {
        [Fact]
        public void GetIntervalFromCustomEventTiming_ValidParamsMorning_ReturnsDateConsideringTimeZone()
        {
            // Arrange
            var dateTime = new DateTime(2021, 8, 1, 10, 0, 0, DateTimeKind.Utc);
            var timezone = "Europe/London";
            
            // Act
            var (start, end) =
                EventTimingMapper.GetIntervalFromCustomEventTiming(dateTime, CustomEventTiming.MORN, timezone);

            // Assert
            start.Hour.Should().Be(5, "Default morning time is 06:00, but should be 05:00 because of the timezone");
            end.Hour.Should().Be(11, "End hour should be 6 hours from the start time");
        }
        
        [Fact]
        public void GetIntervalFromCustomEventTiming_ValidParamsNight_ReturnsDateConsideringTimeZone()
        {
            // Arrange
            var dateTime = new DateTime(2021, 8, 1, 10, 0, 0, DateTimeKind.Utc);
            var timezone = "America/La_Paz";
            
            // Act
            var (start, end) =
                EventTimingMapper.GetIntervalFromCustomEventTiming(dateTime, CustomEventTiming.NIGHT, timezone);

            // Assert
            start.Hour.Should().Be(22, "Default night time is 18:00, but should be 22:00 because of the timezone");
            end.Hour.Should().Be(10, "End hour should be 12 hours from the start time; 10:00 AM of the next day");
        }
        
        [Fact]
        public void GetIntervalFromCustomEventTiming_ValidParamsDefaultTimezone_ReturnsDate()
        {
            // Arrange
            var dateTime = new DateTime(2021, 8, 1, 10, 0, 0, DateTimeKind.Utc);
            var timezone = "UTC";
            
            // Act
            var (start, end) =
                EventTimingMapper.GetIntervalFromCustomEventTiming(dateTime, CustomEventTiming.MORN, timezone);

            // Assert
            start.Hour.Should().Be(6, "Default morning time is 06:00");
            end.Hour.Should().Be(12, "End hour should be 6 hours from the start time");
        }

        [Fact]
        public void ParseTimingFromExtension_ShouldParse()
        {
            // Arrange
            var extension = new Extension("http://localhost/observationTiming", new Code("ACM"));
            var observationTiming = CustomEventTiming.EXACT;
            
            // Act
            if (extension.Url.Contains("Timing") && extension.Value is Code code && TryParse<CustomEventTiming>(code.ToString(), out var temp))
            {
                observationTiming = temp;
            }
            
            // Assert
            observationTiming.Should().Be(CustomEventTiming.ACM);
        }
        
        [Fact]
        public void ParseTimingFromExtension_ShouldNotParse()
        {
            // Arrange
            var extension = new Extension("http://localhost/observationTiming", new Code("INVALID"));
            var observationTiming = CustomEventTiming.EXACT;

            // Act
            if (extension.Url.Contains("Timing") && extension.Value is Code code && TryParse<CustomEventTiming>(code.ToString(), out var temp))
            {
                observationTiming = temp;
            }

            // Assert
            observationTiming.Should().Be(CustomEventTiming.EXACT);
        }
    }
}