﻿namespace QMUL.DiabetesBackend.Controllers.Tests.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json;
    using Api.Controllers;
    using FluentAssertions;
    using Hl7.Fhir.Model;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using NSubstitute;
    using NSubstitute.ExceptionExtensions;
    using ServiceInterfaces;
    using Xunit;
    using Task = System.Threading.Tasks.Task;

    public class MedicationRequestControllerTest
    {
        [Fact]
        public async Task GetMedicationRequest_WhenRequestIsCorrect_ReturnsOk()
        {
            // Arrange
            var service = Substitute.For<IMedicationRequestService>();
            var logger = Substitute.For<ILogger<MedicationRequestController>>();
            service.GetMedicationRequest(Arg.Any<string>()).Returns(new MedicationRequest());
            var controller = new MedicationRequestController(service, logger);

            // Act
            var medicationRequest = await controller.GetMedicationRequest(Guid.NewGuid().ToString());
            var result = (ObjectResult) medicationRequest;

            // Assert
            result.StatusCode.Should().Be(StatusCodes.Status200OK);
        }

        [Fact]
        public async Task GetMedicationRequest_WhenIdDoesNotExists_ReturnsNotFound()
        {
            // Arrange
            var service = Substitute.For<IMedicationRequestService>();
            var logger = Substitute.For<ILogger<MedicationRequestController>>();
            service.GetMedicationRequest(Arg.Any<string>()).Throws(new KeyNotFoundException());
            var controller = new MedicationRequestController(service, logger);

            // Act
            var medicationRequest = await controller.GetMedicationRequest(Guid.NewGuid().ToString());
            var result = (StatusCodeResult) medicationRequest;

            // Assert
            result.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        }

        [Fact]
        public async Task GetMedicationRequest_WhenRequestFails_ReturnsInternalError()
        {
            // Arrange
            var service = Substitute.For<IMedicationRequestService>();
            var logger = Substitute.For<ILogger<MedicationRequestController>>();
            service.GetMedicationRequest(Arg.Any<string>()).Throws(new Exception());
            var controller = new MedicationRequestController(service, logger);

            // Act
            var medicationRequest = await controller.GetMedicationRequest(Guid.NewGuid().ToString());
            var result = (StatusCodeResult) medicationRequest;

            // Assert
            result.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        }

        [Fact]
        public async Task CreateMedicationRequest_WhenRequestIsCorrect_ReturnsStatusOk()
        {
            // Arrange
            var service = Substitute.For<IMedicationRequestService>();
            var logger = Substitute.For<ILogger<MedicationRequestController>>();
            var medicationRequest = new MedicationRequest { Id = Guid.NewGuid().ToString()};
            service.CreateMedicationRequest(Arg.Any<MedicationRequest>()).Returns(medicationRequest);
            var controller = new MedicationRequestController(service, logger);
            var jsonMedicationRequest = JsonSerializer.Serialize(medicationRequest);

            // Act
            var medicationRequestCreated = await controller.CreateMedicationRequest(jsonMedicationRequest);
            var result = (ObjectResult) medicationRequestCreated;

            // Assert
            result.StatusCode.Should().Be(StatusCodes.Status200OK);
        }

        [Fact]
        public async Task CreateMedicationRequest_WhenRequestIsUnformatted_ReturnsBadRequest()
        {
            // Arrange
            var service = Substitute.For<IMedicationRequestService>();
            var logger = Substitute.For<ILogger<MedicationRequestController>>();
            var medicationRequest = new MedicationRequest { Id = Guid.NewGuid().ToString() };
            service.CreateMedicationRequest(Arg.Any<MedicationRequest>()).Returns(medicationRequest);
            var controller = new MedicationRequestController(service, logger);

            // Act
            var medicationRequestCreated = await controller.CreateMedicationRequest("invalid json");
            var result = (StatusCodeResult) medicationRequestCreated;

            // Assert
            result.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        }

        [Fact]
        public async Task CreateMedicationRequest_WhenRequestFails_ReturnsInternalError()
        {
            // Arrange
            var service = Substitute.For<IMedicationRequestService>();
            var logger = Substitute.For<ILogger<MedicationRequestController>>();
            service.CreateMedicationRequest(Arg.Any<MedicationRequest>()).Throws(new Exception());
            var controller = new MedicationRequestController(service, logger);
            var medicationRequest = new MedicationRequest { Id = Guid.NewGuid().ToString() };
            var jsonMedicationRequest = JsonSerializer.Serialize(medicationRequest);

            // Act
            var medicationRequestCreated = await controller.CreateMedicationRequest(jsonMedicationRequest);
            var result = (StatusCodeResult) medicationRequestCreated;

            // Assert
            result.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        }

        [Fact]
        public async Task UpdateMedicationRequest_WhenRequestIsCorrect_ReturnsStatusOk()
        {
            // Arrange
            var id = Guid.NewGuid().ToString();
            var service = Substitute.For<IMedicationRequestService>();
            var logger = Substitute.For<ILogger<MedicationRequestController>>();
            var medicationRequest = new MedicationRequest { Id = id };
            service.UpdateMedicationRequest(Arg.Any<string>(), Arg.Any<MedicationRequest>()).Returns(medicationRequest);
            var controller = new MedicationRequestController(service, logger);
            var jsonMedicationRequest = JsonSerializer.Serialize(medicationRequest);

            // Act
            var medicationRequestCreated = await controller.UpdateMedicationRequest(id, jsonMedicationRequest);
            var result = (ObjectResult) medicationRequestCreated;

            // Assert
            result.StatusCode.Should().Be(StatusCodes.Status202Accepted);
        }

        [Fact]
        public async Task UpdateMedicationRequest_WhenRequestIsUnformatted_ReturnsBadRequest()
        {
            // Arrange
            var id = Guid.NewGuid().ToString();
            var service = Substitute.For<IMedicationRequestService>();
            var logger = Substitute.For<ILogger<MedicationRequestController>>();
            var medicationRequest = new MedicationRequest { Id = id };
            service.UpdateMedicationRequest(Arg.Any<string>(), Arg.Any<MedicationRequest>()).Returns(medicationRequest);
            var controller = new MedicationRequestController(service, logger);

            // Act
            var medicationRequestCreated = await controller.UpdateMedicationRequest(id, "invalid json");
            var result = (StatusCodeResult) medicationRequestCreated;

            // Assert
            result.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        }

        [Fact]
        public async Task UpdateMedicationRequest_WhenRequestIdDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var id = Guid.NewGuid().ToString();
            var service = Substitute.For<IMedicationRequestService>();
            var logger = Substitute.For<ILogger<MedicationRequestController>>();
            service.UpdateMedicationRequest(Arg.Any<string>(), Arg.Any<MedicationRequest>()).Throws(new KeyNotFoundException());
            var controller = new MedicationRequestController(service, logger);
            var medicationRequest = new MedicationRequest { Id = id };
            var jsonMedicationRequest = JsonSerializer.Serialize(medicationRequest);

            // Act
            var medicationRequestCreated = await controller.UpdateMedicationRequest(id, jsonMedicationRequest);
            var result = (StatusCodeResult) medicationRequestCreated;

            // Assert
            result.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        }

        [Fact]
        public async Task UpdateMedicationRequest_WhenRequestFails_ReturnsInternalError()
        {
            // Arrange
            var id = Guid.NewGuid().ToString();
            var service = Substitute.For<IMedicationRequestService>();
            var logger = Substitute.For<ILogger<MedicationRequestController>>();
            service.UpdateMedicationRequest(Arg.Any<string>(), Arg.Any<MedicationRequest>()).Throws(new Exception());
            var controller = new MedicationRequestController(service, logger);
            var medicationRequest = new MedicationRequest { Id = id };
            var jsonMedicationRequest = JsonSerializer.Serialize(medicationRequest);

            // Act
            var medicationRequestCreated = await controller.UpdateMedicationRequest(id, jsonMedicationRequest);
            var result = (StatusCodeResult) medicationRequestCreated;

            // Assert
            result.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        }
        
        [Fact]
        public async Task DeleteMedicationRequest_WhenRequestIsCorrect_ReturnsNoContent()
        {
            // Arrange
            var id = Guid.NewGuid().ToString();
            var service = Substitute.For<IMedicationRequestService>();
            var logger = Substitute.For<ILogger<MedicationRequestController>>();
            service.DeleteMedicationRequest(Arg.Any<string>()).Returns(true);
            var controller = new MedicationRequestController(service, logger);

            // Act
            var medicationRequestCreated = await controller.DeleteMedicationRequest(id);
            var result = (StatusCodeResult) medicationRequestCreated;

            // Assert
            result.StatusCode.Should().Be(StatusCodes.Status204NoContent);
        }

        [Fact]
        public async Task DeleteMedicationRequest_WhenIdDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var id = Guid.NewGuid().ToString();
            var service = Substitute.For<IMedicationRequestService>();
            var logger = Substitute.For<ILogger<MedicationRequestController>>();
            service.DeleteMedicationRequest(Arg.Any<string>()).Throws(new KeyNotFoundException());
            var controller = new MedicationRequestController(service, logger);

            // Act
            var medicationRequestCreated = await controller.DeleteMedicationRequest(id);
            var result = (StatusCodeResult) medicationRequestCreated;

            // Assert
            result.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        }

        [Fact]
        public async Task DeleteMedicationRequest_WhenRequestFails_ReturnsInternalError()
        {
            // Arrange
            var id = Guid.NewGuid().ToString();
            var service = Substitute.For<IMedicationRequestService>();
            var logger = Substitute.For<ILogger<MedicationRequestController>>();
            service.DeleteMedicationRequest(Arg.Any<string>()).Throws(new Exception());
            var controller = new MedicationRequestController(service, logger);

            // Act
            var medicationRequestCreated = await controller.DeleteMedicationRequest(id);
            var result = (StatusCodeResult) medicationRequestCreated;

            // Assert
            result.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        }
    }
}