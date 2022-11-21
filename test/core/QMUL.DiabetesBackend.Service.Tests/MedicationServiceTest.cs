namespace QMUL.DiabetesBackend.Service.Tests;

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

public class MedicationServiceTest
{
    [Fact]
    public async Task GetMedicationList_WhenRequestIsSuccessful_ReturnsBundle()
    {
        // Arrange
        var medicationDao = Substitute.For<IMedicationDao>();
        var logger = Substitute.For<ILogger<MedicationService>>();
        var medicationService = new MedicationService(medicationDao, logger);
        var paginatedResult = new PaginatedResult<IEnumerable<Resource>>
        {
            Results = new Collection<Medication>()
        };

        medicationDao.GetMedicationList(Arg.Any<PaginationRequest>()).Returns(paginatedResult);

        // Act
        await medicationService.GetMedicationList(new PaginationRequest(20, string.Empty));

        // Assert
        await medicationDao.Received(1).GetMedicationList(Arg.Any<PaginationRequest>());
    }

    [Fact]
    public async Task GetSingleMedication_WhenMedicationExists_ReturnsMedication()
    {
        // Arrange
        var medicationDao = Substitute.For<IMedicationDao>();
        var logger = Substitute.For<ILogger<MedicationService>>();
        var medicationService = new MedicationService(medicationDao, logger);

        medicationDao.GetSingleMedication(Arg.Any<string>()).Returns(new Medication());

        // Act
        var result = await medicationService.GetMedication(Guid.NewGuid().ToString());

        // Assert
        result.Should().BeOfType<Medication>();
        await medicationDao.Received(1).GetSingleMedication(Arg.Any<string>());
    }

    [Fact]
    public async Task CreateMedication_WhenMedicationIsCreated_ReturnsNewMedication()
    {
        // Arrange
        var medicationDao = Substitute.For<IMedicationDao>();
        var logger = Substitute.For<ILogger<MedicationService>>();
        var medicationService = new MedicationService(medicationDao, logger);

        medicationDao.CreateMedication(Arg.Any<Medication>()).Returns(new Medication());

        // Act
        var result = await medicationService.CreateMedication(new Medication());

        // Assert
        result.Should().BeOfType<Medication>();
        await medicationDao.Received(1).CreateMedication(Arg.Any<Medication>());
    }
}