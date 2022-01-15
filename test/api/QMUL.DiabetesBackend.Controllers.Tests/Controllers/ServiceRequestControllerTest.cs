namespace QMUL.DiabetesBackend.Controllers.Tests.Controllers
{
    using System;
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

    public class ServiceRequestControllerTest
    {
        [Fact]
        public async Task GetServiceRequest_WhenRequestIsCorrect_ReturnsStatusOk()
        {
            // Arrange
            var service = Substitute.For<IServiceRequestService>();
            var logger = Substitute.For<ILogger<ServiceRequestController>>();
            var controller = new ServiceRequestController(service, logger);
            service.GetServiceRequest(Arg.Any<string>()).Returns(new ServiceRequest());

            // Act
            var serviceRequest = await controller.GetServiceRequest(Guid.NewGuid().ToString());
            var result = (ObjectResult) serviceRequest;

            // Assert
            result.StatusCode.Should().Be(StatusCodes.Status200OK);
        }

        [Fact]
        public async Task GetServiceRequest_WhenRequestFails_ReturnsInternalError()
        {
            // Arrange
            var service = Substitute.For<IServiceRequestService>();
            var logger = Substitute.For<ILogger<ServiceRequestController>>();
            var controller = new ServiceRequestController(service, logger);
            service.GetServiceRequest(Arg.Any<string>()).Throws(new Exception());

            // Act
            var serviceRequest = await controller.GetServiceRequest(Guid.NewGuid().ToString());
            var result = (StatusCodeResult) serviceRequest;

            // Assert
            result.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        }

        [Fact]
        public async Task CreateServiceRequest_WhenRequestIsCorrect_ReturnsStatusOk()
        {
            // Arrange
            var service = Substitute.For<IServiceRequestService>();
            var logger = Substitute.For<ILogger<ServiceRequestController>>();
            var controller = new ServiceRequestController(service, logger);
            var jsonService = JsonSerializer.Serialize(new ServiceRequest());
            service.CreateServiceRequest(Arg.Any<ServiceRequest>()).Returns(new ServiceRequest());

            // Act
            var serviceRequest = await controller.CreateServiceRequest(jsonService);
            var result = (ObjectResult) serviceRequest;

            // Assert
            result.StatusCode.Should().Be(StatusCodes.Status200OK);
        }

        [Fact]
        public async Task CreateServiceRequest_WhenBodyIsUnformatted_ReturnsBadRequest()
        {
            // Arrange
            var service = Substitute.For<IServiceRequestService>();
            var logger = Substitute.For<ILogger<ServiceRequestController>>();
            var controller = new ServiceRequestController(service, logger);

            // Act
            var serviceRequest = await controller.CreateServiceRequest("invalid json");
            var result = (StatusCodeResult) serviceRequest;

            // Assert
            result.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        }

        [Fact]
        public async Task CreateServiceRequest_WhenRequestFails_ReturnsInternalError()
        {
            // Arrange
            var service = Substitute.For<IServiceRequestService>();
            var logger = Substitute.For<ILogger<ServiceRequestController>>();
            var controller = new ServiceRequestController(service, logger);
            service.CreateServiceRequest(Arg.Any<ServiceRequest>()).Throws(new Exception());
            var jsonService = JsonSerializer.Serialize(new ServiceRequest());

            // Act
            var serviceRequest = await controller.CreateServiceRequest(jsonService);
            var result = (StatusCodeResult) serviceRequest;

            // Assert
            result.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        }

        [Fact]
        public async Task UpdateServiceRequest_WhenRequestIsCorrect_ReturnsAccepted()
        {
            // Arrange
            var service = Substitute.For<IServiceRequestService>();
            var logger = Substitute.For<ILogger<ServiceRequestController>>();
            var controller = new ServiceRequestController(service, logger);
            var id = Guid.NewGuid().ToString();
            var jsonService = JsonSerializer.Serialize(new ServiceRequest());
            service.UpdateServiceRequest(Arg.Any<string>(), Arg.Any<ServiceRequest>())
                .Returns(new ServiceRequest());

            // Act
            var serviceRequest = await controller.UpdateServiceRequest(id, jsonService);
            var result = (ObjectResult) serviceRequest;

            // Assert
            result.StatusCode.Should().Be(StatusCodes.Status202Accepted);
        }

        [Fact]
        public async Task UpdateServiceRequest_WhenRequestFails_ReturnsInternalError()
        {
            // Arrange
            var service = Substitute.For<IServiceRequestService>();
            var logger = Substitute.For<ILogger<ServiceRequestController>>();
            var controller = new ServiceRequestController(service, logger);
            var jsonService = JsonSerializer.Serialize(new ServiceRequest());
            service.UpdateServiceRequest(Arg.Any<string>(), Arg.Any<ServiceRequest>())
                .Throws(new Exception());

            // Act
            var serviceRequest = await controller.UpdateServiceRequest(Guid.NewGuid().ToString(), jsonService);
            var result = (StatusCodeResult) serviceRequest;

            // Assert
            result.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        }

        [Fact]
        public async Task DeleteActionResult_WhenRequestIsCorrect_ReturnsNoContent()
        {
            // Arrange
            var service = Substitute.For<IServiceRequestService>();
            var logger = Substitute.For<ILogger<ServiceRequestController>>();
            var controller = new ServiceRequestController(service, logger);
            var id = Guid.NewGuid().ToString();
            service.DeleteServiceRequest(Arg.Any<string>()).Returns(true);

            // Act
            var serviceRequest = await controller.DeleteActionResult(id);
            var result = (StatusCodeResult) serviceRequest;

            // Assert
            result.StatusCode.Should().Be(StatusCodes.Status204NoContent);
        }

        [Fact]
        public async Task DeleteActionResult_WhenRequestFails_ReturnsInternalError()
        {
            // Arrange
            var service = Substitute.For<IServiceRequestService>();
            var logger = Substitute.For<ILogger<ServiceRequestController>>();
            var controller = new ServiceRequestController(service, logger);
            var id = Guid.NewGuid().ToString();
            service.DeleteServiceRequest(Arg.Any<string>()).Throws(new Exception());

            // Act
            var serviceRequest = await controller.DeleteActionResult(id);
            var result = (StatusCodeResult) serviceRequest;

            // Assert
            result.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        }
    }
}