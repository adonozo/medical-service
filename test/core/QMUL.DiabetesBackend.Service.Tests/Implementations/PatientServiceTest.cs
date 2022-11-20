namespace QMUL.DiabetesBackend.Service.Tests.Implementations;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using DataInterfaces;
using FluentAssertions;
using Hl7.Fhir.Model;
using Microsoft.Extensions.Logging;
using Model;
using NSubstitute;
using Service;
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
        var paginatedResult = new PaginatedResult<IEnumerable<Resource>>
        {
            Results = new Collection<Patient>()
        };

        patientDao.GetPatients(Arg.Any<PaginationRequest>()).Returns(paginatedResult);

        // Act
        await patientService.GetPatientList(new PaginationRequest(20, string.Empty));

        // Assert
        await patientDao.Received(1).GetPatients(Arg.Any<PaginationRequest>());
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
    public async Task UpdatePatient_WhenPatientIsUpdated_CallsDaoMethod()
    {
        // Arrange
        var patientDao = Substitute.For<IPatientDao>();
        var logger = Substitute.For<ILogger<PatientService>>();
        var patientService = new PatientService(patientDao, logger);

        patientDao.GetPatientByIdOrEmail(Arg.Any<string>()).Returns(new Patient());
        patientDao.UpdatePatient(Arg.Any<Patient>()).Returns(Task.FromResult(true));

        // Act
        await patientService.UpdatePatient(Guid.NewGuid().ToString(), new Patient());

        // Assert
        await patientDao.Received(1).UpdatePatient(Arg.Any<Patient>());
    }
}