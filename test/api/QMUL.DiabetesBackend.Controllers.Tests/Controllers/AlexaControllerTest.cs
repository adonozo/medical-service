namespace QMUL.DiabetesBackend.Controllers.Tests.Controllers
{
    using System;
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
        public async Task GetMedicationRequest_WhenRequestIsCorrect_ReturnsStatusOk()
        {
            // Arrange
            var service = Substitute.For<IAlexaService>();
            var logger = Substitute.For<ILogger<AlexaController>>();
            service.ProcessMedicationRequest(Arg.Any<string>(), Arg.Any<DateTime>(),
                    Arg.Any<CustomEventTiming>(), Arg.Any<string>())
                .Returns(new Bundle());
            var controller = new AlexaController(service);

            // Act
            var result = await controller.GetMedicationRequest("test@mail.com", DateTime.Now,
                default, CustomEventTiming.ALL_DAY);
            var status = (ObjectResult) result;

            // Assert
            status.StatusCode.Should().Be(StatusCodes.Status200OK);
        }

        [Fact]
        public async Task GetInsulinMedicationRequest_WhenRequestIsCorrect_ReturnsStatusOk()
        {
            // Arrange
            var service = Substitute.For<IAlexaService>();
            var logger = Substitute.For<ILogger<AlexaController>>();
            service.ProcessInsulinMedicationRequest(Arg.Any<string>(), Arg.Any<DateTime>(),
                    Arg.Any<CustomEventTiming>(), Arg.Any<string>())
                .Returns(new Bundle());
            var controller = new AlexaController(service);

            // Act
            var result = await controller.GetInsulinMedicationRequest("test@mail.com", DateTime.Now,
                default, CustomEventTiming.ALL_DAY);
            var status = (ObjectResult) result;

            // Assert
            status.StatusCode.Should().Be(StatusCodes.Status200OK);
        }

        [Fact]
        public async Task GetGlucoseServiceRequest_WhenRequestIsCorrect_ReturnsStatusOk()
        {
            // Arrange
            var service = Substitute.For<IAlexaService>();
            var logger = Substitute.For<ILogger<AlexaController>>();
            service.ProcessGlucoseServiceRequest(Arg.Any<string>(), Arg.Any<DateTime>(),
                    Arg.Any<CustomEventTiming>(), Arg.Any<string>())
                .Returns(new Bundle());
            var controller = new AlexaController(service);

            // Act
            var result = await controller.GetGlucoseServiceRequest("test@mail.com", DateTime.Now,
                default, CustomEventTiming.ALL_DAY);
            var status = (ObjectResult) result;

            // Assert
            status.StatusCode.Should().Be(StatusCodes.Status200OK);
        }

        [Fact]
        public async Task GetCarePlan_WhenRequestIsCorrect_ReturnsStatusOk()
        {
            // Arrange
            var service = Substitute.For<IAlexaService>();
            var logger = Substitute.For<ILogger<AlexaController>>();
            service.ProcessCarePlanRequest(Arg.Any<string>(), Arg.Any<DateTime>(),
                    Arg.Any<CustomEventTiming>(), Arg.Any<string>())
                .Returns(new Bundle());
            var controller = new AlexaController(service);

            // Act
            var result = await controller.GetCarePlan("test@mail.com", DateTime.Now,
                default, CustomEventTiming.ALL_DAY);
            var status = (ObjectResult) result;

            // Assert
            status.StatusCode.Should().Be(StatusCodes.Status200OK);
        }

        [Fact]
        public async Task GetAlexaNextRequest_WhenRequestIsCorrect_ReturnsStatusOk()
        {
            // Arrange
            var service = Substitute.For<IAlexaService>();
            var logger = Substitute.For<ILogger<AlexaController>>();
            service.GetNextRequests(Arg.Any<string>(), Arg.Any<AlexaRequestType>()).Returns(new Bundle());
            var controller = new AlexaController(service);

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
            service.GetNextRequests(Arg.Any<string>(), Arg.Any<AlexaRequestType>())
                .Returns(Task.FromResult<Bundle>(null));
            var controller = new AlexaController(service);

            // Act
            var result = await controller.GetAlexaNextRequest("test@gmail.com", AlexaRequestType.Medication);
            var status = (StatusCodeResult) result;

            // Assert
            status.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        }
    }
}