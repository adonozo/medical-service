namespace QMUL.DiabetesBackend.Service.Tests;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using DataInterfaces;
using FluentAssertions;
using Hl7.Fhir.Model;
using Microsoft.Extensions.Logging;
using Model;
using Model.Enums;
using NodaTime;
using NSubstitute;
using Service;
using Xunit;
using Instant = NodaTime.Instant;
using ResourceReference = Hl7.Fhir.Model.ResourceReference;
using Task = System.Threading.Tasks.Task;

public class ObservationServiceTest
{
    [Fact]
    public async Task CreateObservation_WhenRequestIsSuccessful_SetsTheObservationSubjectId()
    {
        // Arrange
        var patientDao = Substitute.For<IPatientDao>();
        var observationDao = Substitute.For<IObservationDao>();
        var logger = Substitute.For<ILogger<ObservationService>>();
        var observationService = new ObservationService(patientDao, observationDao, logger);

        var patient = TestUtils.GetStubPatient();
        var observation = new Observation { Subject = new ResourceReference() };
        var observationUsedInMethod = new Observation();
        patientDao.GetPatientByIdOrEmail(Arg.Any<string>()).Returns(patient);
        observationDao.CreateObservation(Arg.Do<Observation>(obs => observationUsedInMethod = obs))
            .Returns(observation);

        // Act
        var result = await observationService.CreateObservation(observation, patient.Id);

        // Assert
        result.Should().BeOfType<Observation>();
        observationUsedInMethod.Subject.Reference.Should().Contain(patient.Id);
        observationUsedInMethod.Subject.Display.Should().Contain(patient.Name[0].Family);
    }

    [Fact]
    public async Task GetSingleObservation_WhenObservationExists_ReturnsObservation()
    {
        // Arrange
        var patientDao = Substitute.For<IPatientDao>();
        var observationDao = Substitute.For<IObservationDao>();
        var logger = Substitute.For<ILogger<ObservationService>>();
        var observationService = new ObservationService(patientDao, observationDao, logger);

        observationDao.GetObservation(Arg.Any<string>()).Returns(new Observation());

        // Act
        var result = await observationService.GetObservation(Guid.NewGuid().ToString());

        // Assert
        result.Should().BeOfType<Observation>();
        await observationDao.Received(1).GetObservation(Arg.Any<string>());
    }

    [Fact]
    public async Task GetAllObservationsFor_WhenPatientExists_ReturnsObservations()
    {
        // Arrange
        var patientDao = Substitute.For<IPatientDao>();
        var observationDao = Substitute.For<IObservationDao>();
        var logger = Substitute.For<ILogger<ObservationService>>();
        var observationService = new ObservationService(patientDao, observationDao, logger);
        var paginatedResult = new PaginatedResult<IEnumerable<Resource>>
        {
            Results = new Collection<Observation>()
        };

        patientDao.GetPatientByIdOrEmail(Arg.Any<string>()).Returns(TestUtils.GetStubPatient());
        observationDao.GetAllObservationsFor(Arg.Any<string>(), Arg.Any<PaginationRequest>())
            .Returns(paginatedResult);

        // Act
        var result = await observationService.GetObservations(Guid.NewGuid().ToString(),
            new PaginationRequest(20, null));

        // Assert
        await observationDao.Received(1).GetAllObservationsFor(Arg.Any<string>(), Arg.Any<PaginationRequest>());
        result.Results.Should().BeOfType<Bundle>();
        result.Results.Type.Should().NotBeNull();
    }

    [Fact]
    public async Task GetObservationsFor_WhenRequestIsSuccessful_ReturnsPaginatedBundle()
    {
        // Arrange
        var patientDao = Substitute.For<IPatientDao>();
        var observationDao = Substitute.For<IObservationDao>();
        var logger = Substitute.For<ILogger<ObservationService>>();
        var observationService = new ObservationService(patientDao, observationDao, logger);
        var paginatedResult = new PaginatedResult<IEnumerable<Resource>>
        {
            Results = new Collection<Observation>()
        };

        patientDao.GetPatientByIdOrEmail(Arg.Any<string>()).Returns(TestUtils.GetStubPatient());
        observationDao.GetObservationsFor(Arg.Any<string>(),
            Arg.Any<PaginationRequest>(),
                Arg.Any<Instant>(),
                Arg.Any<Instant>())
            .Returns(paginatedResult);

        // Act
        var result = await observationService.GetObservationsFor(Guid.NewGuid().ToString(),
            CustomEventTiming.AFT,
            new LocalDate(2023, 01, 01),
            new PaginationRequest(20, null));

        // Assert
        await observationDao.Received(1).GetObservationsFor(Arg.Any<string>(),
            Arg.Any<PaginationRequest>(),
            Arg.Any<Instant>(),
            Arg.Any<Instant>());
        result.Results.Should().BeOfType<Bundle>();
        result.Results.Type.Should().NotBeNull();
    }

    [Fact(Skip = "CustomEventTiming.EXACT might be removed soon")]
    public async Task GetObservationsFor_WhenTimingIsExact_SetsTimesWithDefaultTime()
    {
        // Arrange
        var patientDao = Substitute.For<IPatientDao>();
        var observationDao = Substitute.For<IObservationDao>();
        var logger = Substitute.For<ILogger<ObservationService>>();
        var observationService = new ObservationService(patientDao, observationDao, logger);

        var defaultTime = (int)typeof(ObservationService)
            .GetField("DefaultOffsetMinutes",
                BindingFlags.Static | BindingFlags.FlattenHierarchy | BindingFlags.NonPublic)
            .GetValue(null);
        var start = Instant.MinValue;
        var end = Instant.MaxValue;
        var testDateTime = new DateTime(2020, 1, 1, 10, 0, 0);
        var expectedStartDateTime = testDateTime.AddMinutes(defaultTime * -1);
        var expectedEndDateTime = testDateTime.AddMinutes(defaultTime);
        var paginatedResult = new PaginatedResult<IEnumerable<Resource>>
        {
            Results = new Collection<Observation>()
        };

        patientDao.GetPatientByIdOrEmail(Arg.Any<string>()).Returns(TestUtils.GetStubPatient());
        observationDao.GetObservationsFor(Arg.Any<string>(),
                Arg.Any<PaginationRequest>(),
                Arg.Do<Instant>(startTime => start = startTime),
                Arg.Do<Instant>(endTime => end = endTime))
            .Returns(paginatedResult);

        // Act
        await observationService.GetObservationsFor(Guid.NewGuid().ToString(),
            CustomEventTiming.EXACT,
            LocalDate.FromDateTime(testDateTime),
            new PaginationRequest(20, null));

        // Assert
        start.Should().Be(Instant.FromDateTimeUtc(expectedStartDateTime));
        end.Should().Be(Instant.FromDateTimeUtc(expectedEndDateTime));
    }
}