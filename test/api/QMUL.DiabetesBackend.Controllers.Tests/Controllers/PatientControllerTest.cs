namespace QMUL.DiabetesBackend.Controllers.Tests.Controllers;

using System;
using DiabetesBackend.Controllers.Controllers;
using FluentAssertions;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Model;
using Model.Enums;
using Model.Exceptions;
using Models;
using Newtonsoft.Json.Linq;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using ServiceInterfaces;
using ServiceInterfaces.Validators;
using Xunit;
using Task = System.Threading.Tasks.Task;

public class PatientControllerTest
{
    [Fact]
    public async Task CreatePatient_WhenRequestIsCorrect_ReturnsStatusOk()
    {
        // Arrange
        var patientService = Substitute.For<IPatientService>();
        patientService.CreatePatient(Arg.Any<Patient>()).Returns(new Patient { Id = Guid.NewGuid().ToString() });
        var controller = this.GetTestPatientController(patientService: patientService);

        // Act
        var createdPatient = await controller.CreatePatient(new Patient().ToJObject());
        var result = (ObjectResult)createdPatient;

        // Assert
        result.StatusCode.Should().Be(StatusCodes.Status200OK);
    }

    [Fact]
    public async Task PostGlucoseObservation_WhenRequestIsCorrect_ReturnsStatusOk()
    {
        // Arrange
        var observationService = Substitute.For<IObservationService>();
        var id = Guid.NewGuid().ToString();
        var jObservation = new Observation { Id = id }.ToJObject();
        observationService.CreateObservation(Arg.Any<Observation>(), Arg.Any<string>())
            .Returns(new Observation());

        var controller = this.GetTestPatientController(observationService: observationService);

        // Act
        var createdObservation = await controller.PostGlucoseObservation(id, jObservation);
        var result = (ObjectResult)createdObservation;

        // Assert
        result.StatusCode.Should().Be(StatusCodes.Status200OK);
    }

    [Fact]
    public async Task PostGlucoseObservation_WhenBodyIsUnformatted_ReturnsBadRequest()
    {
        // Arrange
        var id = Guid.NewGuid().ToString();
        var validator = Substitute.For<IResourceValidator<Observation>>();
        validator.ParseAndValidateAsync(Arg.Any<JObject>())
            .Throws(new ValidationException(string.Empty));

        var controller = this.GetTestPatientController(observationValidator: validator);

        // Act
        var createdObservation =
            await controller.PostGlucoseObservation(id, JObject.FromObject(new InternalPatient()));
        var result = (ObjectResult)createdObservation;

        // Assert
        result.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task PostGlucoseObservation_WhenRequestFails_ReturnsInternalError()
    {
        // Arrange
        var observationService = Substitute.For<IObservationService>();
        var id = Guid.NewGuid().ToString();
        var jObservation = new Observation { Id = id }.ToJObject();
        observationService.CreateObservation(Arg.Any<Observation>(), Arg.Any<string>())
            .Throws(new Exception());

        var controller = this.GetTestPatientController(observationService: observationService);

        // Act
        var createdObservation = await controller.PostGlucoseObservation(id, jObservation);
        var result = (StatusCodeResult)createdObservation;

        // Assert
        result.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }

    [Fact]
    public async Task GetPatients_WhenRequestIsCorrect_ReturnsStatusOk()
    {
        // Arrange
        var patientService = Substitute.For<IPatientService>();
        var paginatedResult = new PaginatedResult<Bundle>
        {
            Results = new Bundle()
        };
        patientService.GetPatientList(Arg.Any<PaginationRequest>()).Returns(paginatedResult);

        var controller = this.GetTestPatientController(patientService: patientService);

        // Act
        var patients = await controller.GetPatients();
        var result = (ObjectResult)patients;

        // Assert
        result.StatusCode.Should().Be(StatusCodes.Status200OK);
    }

    [Fact]
    public async Task GetPatient_WhenRequestIsCorrect_ReturnsStatusOk()
    {
        // Arrange
        var patientService = Substitute.For<IPatientService>();
        patientService.GetPatient(Arg.Any<string>()).Returns(new Patient());

        var controller = this.GetTestPatientController(patientService: patientService);

        // Act
        var patient = await controller.GetPatient("john@mail.com");
        var result = (ObjectResult)patient;

        // Assert
        result.StatusCode.Should().Be(StatusCodes.Status200OK);
    }

    [Fact]
    public async Task GetPatientCarePlans_WhenRequestIsCorrect_ReturnsStatusOk()
    {
        // Arrange
        var carePlanService = Substitute.For<ICarePlanService>();
        carePlanService.GetCarePlanFor(Arg.Any<string>()).Returns(new Bundle());

        var controller = this.GetTestPatientController(carePlanService: carePlanService);

        // Act
        var carePlans = await controller.GetPatientCarePlans("john@mail.com");
        var result = (ObjectResult)carePlans;

        // Assert
        result.StatusCode.Should().Be(StatusCodes.Status200OK);
    }

    [Fact]
    public async Task GetActiveMedicationRequests_WhenRequestIsCorrect_ReturnsStatusOk()
    {
        // Arrange
        var medicationRequestService = Substitute.For<IMedicationRequestService>();
        var paginatedResult = new PaginatedResult<Bundle>
        {
            Results = new Bundle()
        };
        medicationRequestService.GetActiveMedicationRequests(Arg.Any<string>(), Arg.Any<PaginationRequest>())
            .Returns(paginatedResult);

        var controller = this.GetTestPatientController(medicationRequestService: medicationRequestService);

        // Act
        var medicationRequests = await controller.GetActiveMedicationRequests("john@mail.com");
        var result = (ObjectResult)medicationRequests;

        // Assert
        result.StatusCode.Should().Be(StatusCodes.Status200OK);
    }

    [Fact]
    public async Task GetActiveCarePlan_WhenRequestIsCorrect_ReturnsStatusOk()
    {
        // Arrange
        var carePlanService = Substitute.For<ICarePlanService>();
        carePlanService.GetActiveCarePlans(Arg.Any<string>()).Returns(new Bundle());

        var controller = this.GetTestPatientController(carePlanService: carePlanService);

        // Act
        var carePlan = await controller.GetActiveCarePlan("john@mail.com");
        var result = (ObjectResult)carePlan;

        // Assert
        result.StatusCode.Should().Be(StatusCodes.Status200OK);
    }

    [Fact]
    public async Task GetPatientObservations_WhenRequestIsCorrect_ReturnsStatusOk()
    {
        // Arrange
        var observationService = Substitute.For<IObservationService>();
        var paginatedResult = new PaginatedResult<Bundle>
        {
            Results = new Bundle()
        };

        observationService.GetObservationsFor(Arg.Any<string>(), Arg.Any<CustomEventTiming>(), Arg.Any<DateTime>(),
                Arg.Any<PaginationRequest>(), Arg.Any<string>())
            .Returns(paginatedResult);

        var controller = this.GetTestPatientController(observationService: observationService);

        // Act
        var observations = await controller.GetPatientObservations("john@mail.com", DateTime.Now);
        var result = (ObjectResult)observations;

        // Assert
        result.StatusCode.Should().Be(StatusCodes.Status200OK);
    }

    [Fact]
    public async Task GetAllPatientObservations_WhenRequestIsCorrect_ReturnsStatusOk()
    {
        // Arrange
        var observationService = Substitute.For<IObservationService>();
        var paginatedResult = new PaginatedResult<Bundle>
        {
            Results = new Bundle()
        };
        observationService.GetObservations(Arg.Any<string>(), Arg.Any<PaginationRequest>())
            .Returns(paginatedResult);

        var controller = this.GetTestPatientController(observationService: observationService);

        // Act
        var observations = await controller.GetAllPatientObservations("john@mail.com");
        var result = (ObjectResult)observations;

        // Assert
        result.StatusCode.Should().Be(StatusCodes.Status200OK);
    }

    [Fact]
    public async Task UpdatePatientTiming_WhenRequestIsCorrect_ReturnsNoContent()
    {
        // Arrange
        var alexaService = Substitute.For<IAlexaService>();
        alexaService.UpsertTimingEvent(Arg.Any<string>(), Arg.Any<CustomEventTiming>(), Arg.Any<DateTime>())
            .Returns(true);

        var controller = this.GetTestPatientController(alexaService: alexaService);

        // Act
        var updated = await controller.UpdatePatientTiming("john@mail.com", new PatientTimingRequest());
        var result = (StatusCodeResult)updated;

        // Assert
        result.StatusCode.Should().Be(StatusCodes.Status204NoContent);
    }

    [Fact]
    public async Task UpdatePatientTiming_WhenDoesNotUpdate_ReturnsBadRequest()
    {
        // Arrange
        var alexaService = Substitute.For<IAlexaService>();
        alexaService.UpsertTimingEvent(Arg.Any<string>(), Arg.Any<CustomEventTiming>(), Arg.Any<DateTime>())
            .Returns(false);

        var controller = this.GetTestPatientController(alexaService: alexaService);

        // Act
        var updated = await controller.UpdatePatientTiming("john@mail.com", new PatientTimingRequest());
        var result = (StatusCodeResult)updated;

        // Assert
        result.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task UpdatePatientTiming_WhenRequestFails_ReturnsInternalError()
    {
        // Arrange
        var alexaService = Substitute.For<IAlexaService>();
        alexaService.UpsertTimingEvent(Arg.Any<string>(), Arg.Any<CustomEventTiming>(), Arg.Any<DateTime>())
            .Throws(new Exception());

        var controller = this.GetTestPatientController(alexaService: alexaService);

        // Act
        var updated = await controller.UpdatePatientTiming("john@mail.com", new PatientTimingRequest());
        var result = (StatusCodeResult)updated;

        // Assert
        result.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }

    [Fact]
    public async Task UpdateDosageStartDate_WhenRequestIsCorrect_ReturnsNoContent()
    {
        // Arrange
        var alexaService = Substitute.For<IAlexaService>();
        alexaService.UpsertDosageStartDate(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<DateTime>())
            .Returns(true);

        var controller = this.GetTestPatientController(alexaService: alexaService);

        // Act
        var updated = await controller.UpdateDosageStartDate("john@mail.com", Guid.NewGuid().ToString(),
            new PatientStartDateRequest());
        var result = (StatusCodeResult)updated;

        // Assert
        result.StatusCode.Should().Be(StatusCodes.Status204NoContent);
    }

    [Fact]
    public async Task UpdateDosageStartDate_WhenRequestFails_ReturnsInternalError()
    {
        // Arrange
        var alexaService = Substitute.For<IAlexaService>();
        alexaService.UpsertDosageStartDate(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<DateTime>())
            .Throws(new Exception());

        var controller = this.GetTestPatientController(alexaService: alexaService);

        // Act
        var updated = await controller.UpdateDosageStartDate("john@mail.com", Guid.NewGuid().ToString(),
            new PatientStartDateRequest());
        var result = (StatusCodeResult)updated;

        // Assert
        result.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }

    [Fact]
    public async Task UpdatePatient_WhenRequestIsCorrect_ReturnsAccepted()
    {
        // Arrange
        var patientService = Substitute.For<IPatientService>();
        patientService.UpdatePatient(Arg.Any<string>(), Arg.Any<Patient>()).Returns(Task.FromResult(true));

        var controller = this.GetTestPatientController(patientService: patientService);

        // Act
        var updated = await controller.UpdatePatient(Guid.NewGuid().ToString(), new Patient().ToJObject());
        var result = (AcceptedResult)updated;

        // Assert
        result.StatusCode.Should().Be(StatusCodes.Status202Accepted);
    }

    private PatientController GetTestPatientController(IPatientService patientService = null,
        IAlexaService alexaService = null,
        ICarePlanService carePlanService = null,
        IObservationService observationService = null,
        IMedicationRequestService medicationRequestService = null,
        IResourceValidator<Observation> observationValidator = null,
        IResourceValidator<Patient> patientValidator = null)
    {
        patientService ??= Substitute.For<IPatientService>();
        alexaService ??= Substitute.For<IAlexaService>();
        carePlanService ??= Substitute.For<ICarePlanService>();
        observationService ??= Substitute.For<IObservationService>();
        medicationRequestService ??= Substitute.For<IMedicationRequestService>();
        observationValidator ??= Substitute.For<IResourceValidator<Observation>>();
        patientValidator ??= Substitute.For<IResourceValidator<Patient>>();
        var logger = Substitute.For<ILogger<PatientController>>();

        return new PatientController(patientService, alexaService, carePlanService, observationService,
            medicationRequestService, observationValidator, patientValidator, logger)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext(),
            }
        };
    }
}