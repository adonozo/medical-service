namespace QMUL.DiabetesBackend.Controllers.Tests.Controllers
{
    using System;
    using System.Collections.Generic;
    using Api.Controllers;
    using FluentAssertions;
    using Hl7.Fhir.Model;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Model.Enums;
    using NSubstitute;
    using NSubstitute.ExceptionExtensions;
    using ServiceInterfaces;
    using Xunit;
    using Task = System.Threading.Tasks.Task;

    public class AlexaControllerTest
    {
        [Fact]
        public async Task GetAlexaNextRequest_WhenRequestIsCorrect_ReturnsStatusOk()
        {
            // Arrange
            var service = Substitute.For<IAlexaService>();
            var logger = Substitute.For<ILogger<AlexaController>>();
            service.GetNextRequests(Arg.Any<string>(), Arg.Any<AlexaRequestType>()).Returns(new Bundle());
            var controller = new AlexaController(service, logger);

            // Act
            var result = await controller.GetAlexaNextRequest("test@gmail.com", AlexaRequestType.Medication);
            var status = (ObjectResult) result;

            // Assert
            status.StatusCode.Should().Be(StatusCodes.Status200OK);
        }
        
        [Fact]
        public async Task GetAlexaNextRequest_WhenNotFound_ReturnsNotFound()
        {
            // Arrange
            var service = Substitute.For<IAlexaService>();
            var logger = Substitute.For<ILogger<AlexaController>>();
            service.GetNextRequests(Arg.Any<string>(), Arg.Any<AlexaRequestType>())
                .Throws(new KeyNotFoundException());
            var controller = new AlexaController(service, logger);

            // Act
            var result = await controller.GetAlexaNextRequest("test@gmail.com", AlexaRequestType.Medication);
            var status = (StatusCodeResult) result;

            // Assert
            status.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        }
        
        [Fact]
        public async Task GetAlexaNextRequest_WhenException_ReturnsInternalError()
        {
            // Arrange
            var service = Substitute.For<IAlexaService>();
            var logger = Substitute.For<ILogger<AlexaController>>();
            service.GetNextRequests(Arg.Any<string>(), Arg.Any<AlexaRequestType>())
                .Throws(new Exception());
            var controller = new AlexaController(service, logger);

            // Act
            var result = await controller.GetAlexaNextRequest("test@gmail.com", AlexaRequestType.Medication);
            var status = (StatusCodeResult) result;

            // Assert
            status.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        }
    }
}