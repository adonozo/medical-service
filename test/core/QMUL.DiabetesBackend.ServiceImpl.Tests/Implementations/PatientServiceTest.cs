namespace QMUL.DiabetesBackend.ServiceImpl.Tests.Implementations
{
    using System;
    using System.Collections.Generic;
    using DataInterfaces;
    using FluentAssertions;
    using Hl7.Fhir.Model;
    using Microsoft.Extensions.Logging;
    using NSubstitute;
    using ServiceImpl.Implementations;
    using Xunit;
    using Task = System.Threading.Tasks.Task;

    public class PatientServiceTest
    {
        [Fact]
        public async Task GetPatientList_WhenRequestIsSuccessful_CallsDaoMethod()
        {
            // Arrange
            var patientDao = Substitute.For<IPatientDao>();
            var logger = Substitute.For<ILogger<PatientService>>();
            var patientService = new PatientService(patientDao, logger);

            patientDao.GetPatients().Returns(new List<Patient>());

            // Act
            await patientService.GetPatientList();

            // Assert
            await patientDao.Received(1).GetPatients();
        }

        [Fact]
        public async Task CreatePatient_WhenRequestIsSuccessful_ReturnsPatient()
        {
            // Arrange
            var patientDao = Substitute.For<IPatientDao>();
            var logger = Substitute.For<ILogger<PatientService>>();
            var patientService = new PatientService(patientDao, logger);

            patientDao.CreatePatient(Arg.Any<Patient>()).Returns(new Patient());

            // Act
            var result = await patientService.CreatePatient(new Patient());

            // Assert
            result.Should().BeOfType<Patient>();
            await patientDao.Received(1).CreatePatient(Arg.Any<Patient>());
        }

        [Fact]
        public async Task GetPatient_WhenPatientExists_ReturnsPatient()
        {
            // Arrange
            var patientDao = Substitute.For<IPatientDao>();
            var logger = Substitute.For<ILogger<PatientService>>();
            var patientService = new PatientService(patientDao, logger);

            patientDao.GetPatientByIdOrEmail(Arg.Any<string>()).Returns(new Patient());

            // Act
            var result = await patientService.GetPatient(Guid.NewGuid().ToString());

            // Assert
            result.Should().BeOfType<Patient>();
            await patientDao.Received(1).GetPatientByIdOrEmail(Arg.Any<string>());
        }

        [Fact]
        public async Task UpdatePatient_WhenPatientIsUpdated_ReturnsPatient()
        {
            // Arrange
            var patientDao = Substitute.For<IPatientDao>();
            var logger = Substitute.For<ILogger<PatientService>>();
            var patientService = new PatientService(patientDao, logger);

            patientDao.GetPatientByIdOrEmail(Arg.Any<string>()).Returns(new Patient());
            patientDao.UpdatePatient(Arg.Any<Patient>()).Returns(new Patient());

            // Act
            var result = await patientService.UpdatePatient(Guid.NewGuid().ToString(), new Patient());

            // Assert
            result.Should().BeOfType<Patient>();
            await patientDao.Received(1).UpdatePatient(Arg.Any<Patient>());
        }
    }
}