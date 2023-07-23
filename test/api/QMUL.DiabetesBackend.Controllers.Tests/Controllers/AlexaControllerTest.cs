namespace QMUL.DiabetesBackend.Controllers.Tests.Controllers;

using System;
using DiabetesBackend.Controllers.Controllers;
using FluentAssertions;
using Hl7.Fhir.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Model;
using Model.Enums;
using NSubstitute;
using Service;
using ServiceInterfaces;
using Xunit;
using Task = System.Threading.Tasks.Task;

public class AlexaControllerTest
{
    [Fact]
    public async Task GetMedicationRequest_WhenRequestIsCorrect_ReturnsStatusOk()
    {
        // Arrange
        var alexaService = Substitute.For<IAlexaService>();
        var observationsService = Substitute.For<IObservationService>();
        alexaService.SearchMedicationRequests(Arg.Any<string>(), Arg.Any<DateTime>(), false,
                Arg.Any<CustomEventTiming>(), Arg.Any<string>())
            .Returns(new Bundle());
        var controller = new AlexaController(alexaService, observationsService);

        // Act
        var result = await controller.GetMedicationRequest(
            idOrEmail: "test@mail.com",
            date: DateTime.Now,
            timing: CustomEventTiming.ALL_DAY);
        var status = (ObjectResult)result;

        // Assert
        status.StatusCode.Should().Be(StatusCodes.Status200OK);
    }

    [Fact]
    public async Task GetGlucoseServiceRequest_WhenRequestIsCorrect_ReturnsStatusOk()
    {
        // Arrange
        var alexaService = Substitute.For<IAlexaService>();
        var observationsService = Substitute.For<IObservationService>();
        alexaService.ProcessGlucoseServiceRequest(Arg.Any<string>(), Arg.Any<DateTime>(),
                Arg.Any<CustomEventTiming>(), Arg.Any<string>())
            .Returns(new Bundle());
        var controller = new AlexaController(alexaService, observationsService);

        // Act
        var result = await controller.GetGlucoseServiceRequest(
            idOrEmail: "test@mail.com",
            date: DateTime.Now,
            timing: CustomEventTiming.ALL_DAY);
        var status = (ObjectResult)result;

        // Assert
        status.StatusCode.Should().Be(StatusCodes.Status200OK);
    }

    [Fact]
    public async Task GetPatientObservations_WhenRequestIsCorrect_ReturnsStatusOk()
    {
        // Arrange
        var alexaService = Substitute.For<IAlexaService>();
        var observationsService = Substitute.For<IObservationService>();
        var paginatedResult = new PaginatedResult<Bundle>
        {
            Results = new Bundle()
        };

        observationsService.GetObservationsFor(Arg.Any<string>(), Arg.Any<CustomEventTiming>(), Arg.Any<DateTime>(),
                Arg.Any<PaginationRequest>(), Arg.Any<string>())
            .Returns(paginatedResult);

        var controller = new AlexaController(alexaService, observationsService)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext(),
            }
        };

        // Act
        var observations =
            await controller.GetPatientObservations("john@mail.com", DateTime.Now, CustomEventTiming.ALL_DAY);
        var result = (ObjectResult)observations;

        // Assert
        result.StatusCode.Should().Be(StatusCodes.Status200OK);
    }
}