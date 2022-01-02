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

    public class MedicationServiceTest
    {
        [Fact]
        public async Task GetMedicationList_WhenRequestIsSuccessful_ReturnsBundle()
        {
            // Arrange
            var medicationDao = Substitute.For<IMedicationDao>();
            var logger = Substitute.For<ILogger<MedicationService>>();
            var medicationService = new MedicationService(medicationDao, logger);

            medicationDao.GetMedicationList().Returns(new List<Medication> { new () });

            // Act
            var result = await medicationService.GetMedicationList();

            // Assert
            result.Should().BeOfType<Bundle>();
            result.Entry.Count.Should().Be(1);
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
            var result = await medicationService.GetSingleMedication(Guid.NewGuid().ToString());

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
}