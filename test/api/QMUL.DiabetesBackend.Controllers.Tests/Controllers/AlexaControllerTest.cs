namespace QMUL.DiabetesBackend.Controllers.Tests.Controllers;

using System;
using DiabetesBackend.Controllers.Controllers;
using FluentAssertions;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Model;
using Model.Enums;
using NodaTime;
using NSubstitute;
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
        var carePlanService = Substitute.For<ICarePlanService>();
        alexaService.SearchMedicationRequests(Arg.Any<string>(),
                Arg.Any<LocalDate>(),
                false,
                Arg.Any<CustomEventTiming>(),
                Arg.Any<string>())
            .Returns(Task.FromResult(Result<Bundle, MedicationRequest>.Success(new Bundle())));
        var controller = new AlexaController(alexaService, observationsService, carePlanService);

        // Act
        var result = await controller.GetMedicationRequests(
            idOrEmail: "test@mail.com",
            date: new LocalDate(),
            timing: CustomEventTiming.ALL_DAY);
        var status = (ObjectResult)result;

        // Assert
        status.StatusCode.Should().Be(StatusCodes.Status200OK);
    }

    [Fact]
    public async Task GetMedicationRequest_WhenDateIsMissing_ReturnsUnprocessableEntity()
    {
        // Arrange
        var alexaService = Substitute.For<IAlexaService>();
        var observationsService = Substitute.For<IObservationService>();
        var carePlanService = Substitute.For<ICarePlanService>();
        alexaService.SearchMedicationRequests(Arg.Any<string>(),
                Arg.Any<LocalDate>(),
                false,
                Arg.Any<CustomEventTiming>(),
                Arg.Any<string>())
            .Returns(Task.FromResult(Result<Bundle, MedicationRequest>.Success(new Bundle())));
        var controller = new AlexaController(alexaService, observationsService, carePlanService);

        // Act
        var result = await controller.GetMedicationRequests(
            idOrEmail: "test@mail.com",
            date: null,
            timing: CustomEventTiming.ALL_DAY);
        var status = (ObjectResult)result;

        // Assert
        status.StatusCode.Should().Be(StatusCodes.Status422UnprocessableEntity);
    }

    [Fact]
    public async Task GetMedicationRequest_WhenResultFails_ReturnsUnprocessableEntity()
    {
        // Arrange
        var alexaService = Substitute.For<IAlexaService>();
        var observationsService = Substitute.For<IObservationService>();
        var carePlanService = Substitute.For<ICarePlanService>();

        var expectedRequest = new MedicationRequest{ Id = Guid.NewGuid().ToString() };
        alexaService.SearchMedicationRequests(Arg.Any<string>(),
                Arg.Any<LocalDate>(),
                false,
                Arg.Any<CustomEventTiming>(),
                Arg.Any<string>())
            .Returns(Task.FromResult(Result<Bundle, MedicationRequest>.Fail(expectedRequest)));
        var controller = new AlexaController(alexaService, observationsService, carePlanService);

        // Act
        var result = await controller.GetMedicationRequests(
            idOrEmail: "test@mail.com",
            date: new LocalDate(2023, 01, 01),
            timing: CustomEventTiming.ALL_DAY);
        var status = (ObjectResult)result;

        // Assert
        status.StatusCode.Should().Be(StatusCodes.Status422UnprocessableEntity);
        var errors = status.Value.Should().BeOfType<ProblemDetails>().Subject;
        errors.Extensions.Should().ContainKey(AlexaController.ResourceErrorKey)
            .WhoseValue.Should().BeEquivalentTo(expectedRequest.ToJObject());
    }

    [Fact]
    public async Task GetGlucoseServiceRequest_WhenRequestIsCorrect_ReturnsStatusOk()
    {
        // Arrange
        var alexaService = Substitute.For<IAlexaService>();
        var observationsService = Substitute.For<IObservationService>();
        var carePlanService = Substitute.For<ICarePlanService>();
        alexaService.SearchActiveServiceRequests(Arg.Any<string>())
            .Returns(Task.FromResult(Result<Bundle, ServiceRequest>.Success(new Bundle())));
        var controller = new AlexaController(alexaService, observationsService, carePlanService);

        // Act
        var result = await controller.GetServiceRequests(idOrEmail: "test@mail.com");
        var status = (ObjectResult)result;

        // Assert
        status.StatusCode.Should().Be(StatusCodes.Status200OK);
    }

    [Fact]
    public async Task GetGlucoseServiceRequest_WhenResultFails_ReturnsUnprocessableEntity()
    {
        // Arrange
        var alexaService = Substitute.For<IAlexaService>();
        var observationsService = Substitute.For<IObservationService>();
        var carePlanService = Substitute.For<ICarePlanService>();

        var expectedFailRequest = new ServiceRequest{ Id = Guid.NewGuid().ToString()};
        alexaService.SearchActiveServiceRequests(Arg.Any<string>())
            .Returns(Task.FromResult(Result<Bundle, ServiceRequest>.Fail(expectedFailRequest)));
        var controller = new AlexaController(alexaService, observationsService, carePlanService);

        // Act
        var result = await controller.GetServiceRequests(idOrEmail: "test@mail.com");
        var status = (ObjectResult)result;

        // Assert
        status.StatusCode.Should().Be(StatusCodes.Status422UnprocessableEntity);
        var errors = status.Value.Should().BeOfType<ProblemDetails>().Subject;
        errors.Extensions.Should().ContainKey(AlexaController.ResourceErrorKey)
            .WhoseValue.Should().BeEquivalentTo(expectedFailRequest.ToJObject());
    }

    [Fact]
    public async Task GetPatientObservations_WhenRequestIsCorrect_ReturnsStatusOk()
    {
        // Arrange
        var alexaService = Substitute.For<IAlexaService>();
        var observationsService = Substitute.For<IObservationService>();
        var carePlanService = Substitute.For<ICarePlanService>();
        var paginatedResult = new PaginatedResult<Bundle>
        {
            Results = new Bundle()
        };

        observationsService.GetObservationsFor(Arg.Any<string>(),
                Arg.Any<CustomEventTiming>(),
                Arg.Any<LocalDate>(),
                Arg.Any<PaginationRequest>(),
                Arg.Any<string>())
            .Returns(paginatedResult);

        var controller = new AlexaController(alexaService, observationsService, carePlanService)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext(),
            }
        };

        // Act
        var observations = await controller.GetPatientObservations("john@mail.com",
            new LocalDate(),
            CustomEventTiming.ALL_DAY);
        var result = (ObjectResult)observations;

        // Assert
        result.StatusCode.Should().Be(StatusCodes.Status200OK);
    }
}