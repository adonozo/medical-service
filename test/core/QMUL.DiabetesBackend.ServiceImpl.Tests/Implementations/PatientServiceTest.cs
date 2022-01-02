namespace QMUL.DiabetesBackend.ServiceImpl.Tests.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using DataInterfaces;
    using FluentAssertions;
    using Microsoft.Extensions.Logging;
    using Model;
    using NSubstitute;
    using ServiceImpl.Implementations;
    using Xunit;

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
            
            // Assert
            var result = await patientService.GetPatient(Guid.NewGuid().ToString());
            
            // Assert
            result.Should().BeOfType<Patient>();
            await patientDao.Received(1).GetPatientByIdOrEmail(Arg.Any<string>());
        }
    }
}