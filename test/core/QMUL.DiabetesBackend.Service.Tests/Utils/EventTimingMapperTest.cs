namespace QMUL.DiabetesBackend.Service.Tests.Utils;

using System.Collections.Generic;
using FluentAssertions;
using Hl7.Fhir.Model;
using Model.Enums;
using NodaTime;
using Service.Utils;
using Xunit;
using static System.Enum;

public class EventTimingMapperTest
{
    [Fact]
    public void GetIntervalFromCustomEventTiming_ValidParamsMorning_ReturnsDateConsideringTimeZone()
    {
        // Arrange
        var dateTime = new LocalDate(2021, 8, 1);
        var timezone = "Europe/London";

        // Act
        var interval = EventTimingMapper.GetDefaultIntervalFromEventTiming(dateTime, CustomEventTiming.MORN, timezone);

        // Assert
        interval.Start.InUtc().Hour.Should().Be(5, "Default morning time is 06:00, but should be 05:00 because of the timezone");
        interval.End.InUtc().Hour.Should().Be(11, "End hour should be 6 hours from the start time");
    }

    [Fact]
    public void GetIntervalFromCustomEventTiming_ValidParamsNight_ReturnsDateConsideringTimeZone()
    {
        // Arrange
        var dateTime = new LocalDate(2021, 8, 1);
        var timezone = "America/La_Paz";

        // Act
        var interval = EventTimingMapper.GetDefaultIntervalFromEventTiming(dateTime, CustomEventTiming.NIGHT, timezone);

        // Assert
        interval.Start.InUtc().Hour.Should().Be(22, "Default night time is 18:00, but should be 22:00 because of the timezone");
        interval.End.InUtc().Hour.Should().Be(7, "End hour should be at 03:00 AM of the next day; 07:00 AM because of the timezone");
    }

    [Fact]
    public void GetIntervalFromCustomEventTiming_ValidParamsDefaultTimezone_ReturnsDate()
    {
        // Arrange
        var dateTime = new LocalDate(2021, 8, 1);
        var timezone = "UTC";

        // Act
        var interval = EventTimingMapper.GetDefaultIntervalFromEventTiming(dateTime, CustomEventTiming.MORN, timezone);

        // Assert
        interval.Start.InUtc().Hour.Should().Be(6, "Default morning time is 06:00");
        interval.End.InUtc().Hour.Should().Be(12, "End hour should be 6 hours from the start time");
    }

    [Fact]
    public void ParseTimingFromValidExtension_ShouldParse()
    {
        // Arrange
        var extension = new Extension("http://localhost/observationTiming", new Code("ACM"));
        var observationTiming = CustomEventTiming.EXACT;

        // Act
        if (extension.Url.Contains("Timing") && extension.Value is Code code &&
            TryParse<CustomEventTiming>(code.ToString(), out var temp))
        {
            observationTiming = temp;
        }

        // Assert
        observationTiming.Should().Be(CustomEventTiming.ACM);
    }

    [Fact]
    public void ParseTimingFromInvalidExtension_ShouldNotParse()
    {
        // Arrange
        var extension = new Extension("http://localhost/observationTiming", new Code("INVALID"));
        var observationTiming = CustomEventTiming.EXACT;

        // Act
        if (extension.Url.Contains("Timing") && extension.Value is Code code &&
            TryParse<CustomEventTiming>(code.ToString(), out var temp))
        {
            observationTiming = temp;
        }

        // Assert
        observationTiming.Should().Be(CustomEventTiming.EXACT);
    }

    [Fact]
    public void GetIntervalForPatient_WhenPatientHasRecords_GetsIntervalFromRecords()
    {
        // Arrange
        var timingEvent = CustomEventTiming.AC;
        var timezone = "UTC";

        var patientTimingRecord = new LocalTime(12, 00, 00);
        var timingPreferences = new Dictionary<CustomEventTiming, LocalTime>
            { { timingEvent, patientTimingRecord } };
        var referenceDate = new LocalDate(2020, 1, 1);


        // Act
        var interval = EventTimingMapper.TimingIntervalForPatient(timingPreferences, referenceDate, timingEvent, timezone, 30);

        // Assert
        interval.Start.InUtc().Hour.Should().Be(11);
        interval.Start.InUtc().Minute.Should().Be(30, "Start date is 30 min (default offset) before patient's timing record");
        interval.End.InUtc().Hour.Should().Be(12);
        interval.End.InUtc().Minute.Should().Be(30, "End date is 30 min (default offset) after patient's timing record");
    }

    [Fact]
    public void GetIntervalForPatient_WhenPatientDoNotHaveRecords_GetsDefaultIntervals()
    {
        // Arrange
        var timingEvent = CustomEventTiming.MORN;
        var timezone = "UTC";

        var referenceDate = new LocalDate(2021, 8, 1);
        var timingPreferences = new Dictionary<CustomEventTiming, LocalTime>();

        // Act
        var interval = EventTimingMapper.TimingIntervalForPatient(timingPreferences, referenceDate, timingEvent, timezone, 30);

        // Assert
        interval.Start.InUtc().Hour.Should().Be(6, "Default morning time is 06:00");
        interval.End.InUtc().Hour.Should().Be(12, "Default End hour is 6 hours from the start time");
    }
}