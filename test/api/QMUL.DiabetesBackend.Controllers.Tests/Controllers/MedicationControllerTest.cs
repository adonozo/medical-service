namespace QMUL.DiabetesBackend.Controllers.Tests.Controllers
{
    using System;
    using System.Text.Json;
    using FluentAssertions;
    using Hl7.Fhir.Model;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using NSubstitute;
    using NSubstitute.ExceptionExtensions;
    using QMUL.DiabetesBackend.Api.Controllers;
    using ServiceInterfaces;
    using Xunit;
    using Task = System.Threading.Tasks.Task;

    public class MedicationControllerTest
    {
        [Fact]
        public async Task GetAllMedications_WhenRequestIsCorrect_ReturnsOk()
        {
            // Arrange
            var service = Substitute.For<IMedicationService>();
            var logger = Substitute.For<ILogger<MedicationController>>();
            service.GetMedicationList().Returns(new Bundle());
            var controller = new MedicationController(service, logger);

            // Act
            var medications = await controller.GetAllMedications();
            var result = (ObjectResult) medications;

            // Assert
            result.StatusCode.Should().Be(StatusCodes.Status200OK);
        }

        [Fact]
        public async Task GetAllMedications_WhenRequestFails_ReturnsInternalError()
        {
            // Arrange
            var service = Substitute.For<IMedicationService>();
            var logger = Substitute.For<ILogger<MedicationController>>();
            service.GetMedicationList().Throws(new Exception());
            var controller = new MedicationController(service, logger);

            // Act
            var medications = await controller.GetAllMedications();
            var result = (StatusCodeResult) medications;

            // Assert
            result.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        }

        [Fact]
        public async Task GetMedicationRequest_WhenRequestIsCorrect_ReturnsOk()
        {
            // Arrange
            var service = Substitute.For<IMedicationService>();
            var logger = Substitute.For<ILogger<MedicationController>>();
            var id = Guid.NewGuid().ToString();
            service.GetSingleMedication(Arg.Any<string>()).Returns(new Medication());
            var controller = new MedicationController(service, logger);

            // Act
            var medication = await controller.GetMedicationRequest(id);
            var result = (ObjectResult) medication;

            // Assert
            result.StatusCode.Should().Be(StatusCodes.Status200OK);
        }

        [Fact]
        public async Task GetMedicationRequest_WhenRequestFails_ReturnsInternalError()
        {
            // Arrange
            var service = Substitute.For<IMedicationService>();
            var logger = Substitute.For<ILogger<MedicationController>>();
            var id = Guid.NewGuid().ToString();
            service.GetSingleMedication(Arg.Any<string>()).Throws(new Exception());
            var controller = new MedicationController(service, logger);

            // Act
            var medications = await controller.GetMedicationRequest(id);
            var result = (StatusCodeResult) medications;

            // Assert
            result.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        }

        [Fact]
        public async Task GetMedication_WhenRequestIsCorrect_ReturnsOk()
        {
            // Arrange
            var service = Substitute.For<IMedicationService>();
            var logger = Substitute.For<ILogger<MedicationController>>();
            var medication = new Medication { Id = Guid.NewGuid().ToString()};
            service.CreateMedication(Arg.Any<Medication>()).Returns(medication);
            var controller = new MedicationController(service, logger);
            var jsonRequest = JsonSerializer.Serialize(medication);

            // Act
            var medicationCreated = await controller.CreateMedication(jsonRequest);
            var result = (ObjectResult) medicationCreated;

            // Assert
            result.StatusCode.Should().Be(StatusCodes.Status200OK);
        }

        [Fact]
        public async Task GetMedication_WhenRequestUnformatted_ReturnsBadRequest()
        {
            // Arrange
            var service = Substitute.For<IMedicationService>();
            var logger = Substitute.For<ILogger<MedicationController>>();
            var medication = new Medication { Id = Guid.NewGuid().ToString() };
            service.CreateMedication(Arg.Any<Medication>()).Returns(medication);
            var controller = new MedicationController(service, logger);

            // Act
            var medicationCreated = await controller.CreateMedication("invalid json");
            var result = (StatusCodeResult) medicationCreated;

            // Assert
            result.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        }

        [Fact]
        public async Task GetMedication_WhenRequestFails_ReturnsInternalError()
        {
            // Arrange
            var service = Substitute.For<IMedicationService>();
            var logger = Substitute.For<ILogger<MedicationController>>();
            service.CreateMedication(Arg.Any<Medication>()).Throws(new Exception());
            var controller = new MedicationController(service, logger);
            var medication = new Medication { Id = Guid.NewGuid().ToString() };
            var jsonRequest = JsonSerializer.Serialize(medication);

            // Act
            var medicationCreated = await controller.CreateMedication(jsonRequest);
            var result = (StatusCodeResult) medicationCreated;

            // Assert
            result.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        }
    }
}
