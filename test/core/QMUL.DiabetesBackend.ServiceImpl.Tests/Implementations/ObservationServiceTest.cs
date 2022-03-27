namespace QMUL.DiabetesBackend.ServiceImpl.Tests.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using DataInterfaces;
    using FluentAssertions;
    using Hl7.Fhir.Model;
    using Microsoft.Extensions.Logging;
    using Model.Enums;
    using NSubstitute;
    using ServiceImpl.Implementations;
    using Xunit;
    using Patient = Model.Patient;
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

            var patient = this.GetDummyPatient();
            var observation = new Observation { Subject = new ResourceReference() };
            var observationUsedInMethod = new Observation();
            patientDao.GetPatientByIdOrEmail(Arg.Any<string>()).Returns(patient);
            observationDao.CreateObservation(Arg.Do<Observation>(obs => observationUsedInMethod = obs))
                .Returns(observation);

            // Act
            var result = await observationService.CreateObservation(patient.Id, observation);

            // Assert
            result.Should().BeOfType<Observation>();
            observationUsedInMethod.Subject.ElementId.Should().Be(patient.Id);
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
            var result = await observationService.GetSingleObservation(Guid.NewGuid().ToString());
            
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

            patientDao.GetPatientByIdOrEmail(Arg.Any<string>()).Returns(this.GetDummyPatient());
            observationDao.GetAllObservationsFor(Arg.Any<string>()).Returns(new List<Observation> { new() });

            // Act
            var result = await observationService.GetAllObservationsFor(Guid.NewGuid().ToString());
            
            // Assert
            result.Should().BeOfType<Bundle>();
            result.Entry.Count.Should().Be(1);
        }

        [Fact]
        public async Task GetObservationsFor_WhenRequestIsSuccessful_ReturnsBundle()
        {
            // Arrange
            var patientDao = Substitute.For<IPatientDao>();
            var observationDao = Substitute.For<IObservationDao>();
            var logger = Substitute.For<ILogger<ObservationService>>();
            var observationService = new ObservationService(patientDao, observationDao, logger);
            
            patientDao.GetPatientByIdOrEmail(Arg.Any<string>()).Returns(this.GetDummyPatient());
            observationDao.GetObservationsFor(Arg.Any<string>(), Arg.Any<DateTime>(), Arg.Any<DateTime>())
                .Returns(new List<Observation> { new() });

            // Act
            var result =
                await observationService.GetObservationsFor(Guid.NewGuid().ToString(), CustomEventTiming.AFT,
                    DateTime.Now);
            
            // Assert
            result.Should().BeOfType<Bundle>();
            result.Entry.Count.Should().Be(1);
        }

        [Fact]
        public async Task GetObservationsFor_WhenTimingIsExact_SetsTimesWithDefaultTime()
        {
            // Arrange
            var patientDao = Substitute.For<IPatientDao>();
            var observationDao = Substitute.For<IObservationDao>();
            var logger = Substitute.For<ILogger<ObservationService>>();
            var observationService = new ObservationService(patientDao, observationDao, logger);

            // ReSharper disable once PossibleNullReferenceException
            var defaultTime = (int)typeof(ObservationService)
                .GetField("DefaultOffset",
                    BindingFlags.Static | BindingFlags.FlattenHierarchy | BindingFlags.NonPublic)
                .GetValue(null);
            DateTime start = default, end = default;
            var testDateTime = new DateTime(2020, 1, 1, 10, 0, 0);
            var expectedStartDateTime = testDateTime.AddMinutes(defaultTime * -1);
            var expectedEndDateTime = testDateTime.AddMinutes(defaultTime);
            
            patientDao.GetPatientByIdOrEmail(Arg.Any<string>()).Returns(this.GetDummyPatient());
            observationDao.GetObservationsFor(Arg.Any<string>(), Arg.Do<DateTime>(startTime => start = startTime),
                    Arg.Do<DateTime>(endTime => end = endTime))
                .Returns(new List<Observation> { new() });

            // Act
            await observationService.GetObservationsFor(Guid.NewGuid().ToString(), CustomEventTiming.EXACT,
                testDateTime);

            // Assert
            start.Should().Be(expectedStartDateTime);
            end.Should().Be(expectedEndDateTime);
        }

        #region Private methods

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