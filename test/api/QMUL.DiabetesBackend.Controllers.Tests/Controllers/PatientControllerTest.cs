namespace QMUL.DiabetesBackend.Controllers.Tests.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json;
    using Api.Controllers;
    using Api.Models;
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
    using Patient = Model.Patient;
    using Task = System.Threading.Tasks.Task;

    public class PatientControllerTest
    {
        [Fact]
        public async Task CreatePatient_WhenRequestIsCorrect_ReturnsStatusOk()
        {
            // Arrange
            var patientService = Substitute.For<IPatientService>();
            var alexaService = Substitute.For<IAlexaService>();
            var carePlanService = Substitute.For<ICarePlanService>();
            var observationService = Substitute.For<IObservationService>();
            var medicationRequestService = Substitute.For<IMedicationRequestService>();
            var logger = Substitute.For<ILogger<PatientController>>();
            var controller = new PatientController(patientService, alexaService, carePlanService, observationService,
                medicationRequestService, logger);
            patientService.CreatePatient(Arg.Any<Patient>()).Returns(new Patient {Id = Guid.NewGuid().ToString()});

            // Act
            var createdPatient = await controller.CreatePatient(new Patient {FirstName = "John", LastName = "Doe"});
            var result = (ObjectResult) createdPatient;

            // Assert
            result.StatusCode.Should().Be(StatusCodes.Status200OK);
        }

        [Fact]
        public async Task PostGlucoseObservation_WhenRequestIsCorrect_ReturnsStatusOk()
        {
            // Arrange
            var patientService = Substitute.For<IPatientService>();
            var alexaService = Substitute.For<IAlexaService>();
            var carePlanService = Substitute.For<ICarePlanService>();
            var observationService = Substitute.For<IObservationService>();
            var medicationRequestService = Substitute.For<IMedicationRequestService>();
            var logger = Substitute.For<ILogger<PatientController>>();
            var controller = new PatientController(patientService, alexaService, carePlanService, observationService,
                medicationRequestService, logger);
            var id = Guid.NewGuid().ToString();
            var jsonObservation = JsonSerializer.Serialize(new Observation {Id = id});
            observationService.CreateObservation(Arg.Any<string>(), Arg.Any<Observation>())
                .Returns(new Observation());

            // Act
            var createdObservation = await controller.PostGlucoseObservation(id, jsonObservation);
            var result = (ObjectResult)createdObservation;

            // Assert
            result.StatusCode.Should().Be(StatusCodes.Status200OK);
        }

        [Fact]
        public async Task PostGlucoseObservation_WhenIdDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var patientService = Substitute.For<IPatientService>();
            var alexaService = Substitute.For<IAlexaService>();
            var carePlanService = Substitute.For<ICarePlanService>();
            var observationService = Substitute.For<IObservationService>();
            var medicationRequestService = Substitute.For<IMedicationRequestService>();
            var logger = Substitute.For<ILogger<PatientController>>();
            var controller = new PatientController(patientService, alexaService, carePlanService, observationService,
                medicationRequestService, logger);
            var id = Guid.NewGuid().ToString();
            var jsonObservation = JsonSerializer.Serialize(new Observation { Id = id });
            observationService.CreateObservation(Arg.Any<string>(), Arg.Any<Observation>())
                .Throws(new KeyNotFoundException());

            // Act
            var createdObservation = await controller.PostGlucoseObservation(id, jsonObservation);
            var result = (StatusCodeResult)createdObservation;

            // Assert
            result.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        }

        [Fact]
        public async Task PostGlucoseObservation_WhenBodyIsUnformatted_ReturnsBadRequest()
        {
            // Arrange
            var patientService = Substitute.For<IPatientService>();
            var alexaService = Substitute.For<IAlexaService>();
            var carePlanService = Substitute.For<ICarePlanService>();
            var observationService = Substitute.For<IObservationService>();
            var medicationRequestService = Substitute.For<IMedicationRequestService>();
            var logger = Substitute.For<ILogger<PatientController>>();
            var controller = new PatientController(patientService, alexaService, carePlanService, observationService,
                medicationRequestService, logger);
            var id = Guid.NewGuid().ToString();
            observationService.CreateObservation(Arg.Any<string>(), Arg.Any<Observation>())
                .Throws(new KeyNotFoundException());

            // Act
            var createdObservation = await controller.PostGlucoseObservation(id, "invalid json");
            var result = (StatusCodeResult)createdObservation;

            // Assert
            result.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        }

        [Fact]
        public async Task PostGlucoseObservation_WhenRequestFails_ReturnsInternalError()
        {
            // Arrange
            var patientService = Substitute.For<IPatientService>();
            var alexaService = Substitute.For<IAlexaService>();
            var carePlanService = Substitute.For<ICarePlanService>();
            var observationService = Substitute.For<IObservationService>();
            var medicationRequestService = Substitute.For<IMedicationRequestService>();
            var logger = Substitute.For<ILogger<PatientController>>();
            var controller = new PatientController(patientService, alexaService, carePlanService, observationService,
                medicationRequestService, logger);
            var id = Guid.NewGuid().ToString();
            var jsonObservation = JsonSerializer.Serialize(new Observation { Id = id });
            observationService.CreateObservation(Arg.Any<string>(), Arg.Any<Observation>())
                .Throws(new Exception());

            // Act
            var createdObservation = await controller.PostGlucoseObservation(id, jsonObservation);
            var result = (StatusCodeResult)createdObservation;

            // Assert
            result.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        }

        [Fact]
        public async Task GetPatients_WhenRequestIsCorrect_ReturnsStatusOk()
        {
            // Arrange
            var patientService = Substitute.For<IPatientService>();
            var alexaService = Substitute.For<IAlexaService>();
            var carePlanService = Substitute.For<ICarePlanService>();
            var observationService = Substitute.For<IObservationService>();
            var medicationRequestService = Substitute.For<IMedicationRequestService>();
            var logger = Substitute.For<ILogger<PatientController>>();
            var controller = new PatientController(patientService, alexaService, carePlanService, observationService,
                medicationRequestService, logger);
            patientService.GetPatientList().Returns(new List<Patient>());

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
            var alexaService = Substitute.For<IAlexaService>();
            var carePlanService = Substitute.For<ICarePlanService>();
            var observationService = Substitute.For<IObservationService>();
            var medicationRequestService = Substitute.For<IMedicationRequestService>();
            var logger = Substitute.For<ILogger<PatientController>>();
            var controller = new PatientController(patientService, alexaService, carePlanService, observationService,
                medicationRequestService, logger);
            patientService.GetPatient(Arg.Any<string>()).Returns(new Patient());

            // Act
            var patient = await controller.GetPatient("john@mail.com");
            var result = (ObjectResult)patient;

            // Assert
            result.StatusCode.Should().Be(StatusCodes.Status200OK);
        }

        [Fact]
        public async Task GetPatient_WhenEmailDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var patientService = Substitute.For<IPatientService>();
            var alexaService = Substitute.For<IAlexaService>();
            var carePlanService = Substitute.For<ICarePlanService>();
            var observationService = Substitute.For<IObservationService>();
            var medicationRequestService = Substitute.For<IMedicationRequestService>();
            var logger = Substitute.For<ILogger<PatientController>>();
            var controller = new PatientController(patientService, alexaService, carePlanService, observationService,
                medicationRequestService, logger);
            patientService.GetPatient(Arg.Any<string>()).Throws(new KeyNotFoundException());

            // Act
            var patient = await controller.GetPatient("john@mail.com");
            var result = (StatusCodeResult)patient;

            // Assert
            result.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        }

        [Fact]
        public async Task GetPatient_WhenRequestFails_ReturnsInternalError()
        {
            // Arrange
            var patientService = Substitute.For<IPatientService>();
            var alexaService = Substitute.For<IAlexaService>();
            var carePlanService = Substitute.For<ICarePlanService>();
            var observationService = Substitute.For<IObservationService>();
            var medicationRequestService = Substitute.For<IMedicationRequestService>();
            var logger = Substitute.For<ILogger<PatientController>>();
            var controller = new PatientController(patientService, alexaService, carePlanService, observationService,
                medicationRequestService, logger);
            patientService.GetPatient(Arg.Any<string>()).Throws(new Exception());

            // Act
            var patient = await controller.GetPatient("john@mail.com");
            var result = (StatusCodeResult)patient;

            // Assert
            result.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        }

        [Fact]
        public async Task GetPatientCarePlans_WhenRequestIsCorrect_ReturnsStatusOk()
        {
            // Arrange
            var patientService = Substitute.For<IPatientService>();
            var alexaService = Substitute.For<IAlexaService>();
            var carePlanService = Substitute.For<ICarePlanService>();
            var observationService = Substitute.For<IObservationService>();
            var medicationRequestService = Substitute.For<IMedicationRequestService>();
            var logger = Substitute.For<ILogger<PatientController>>();
            var controller = new PatientController(patientService, alexaService, carePlanService, observationService,
                medicationRequestService, logger);
            carePlanService.GetCarePlanFor(Arg.Any<string>()).Returns(new Bundle());

            // Act
            var carePlans = await controller.GetPatientCarePlans("john@mail.com");
            var result = (ObjectResult)carePlans;

            // Assert
            result.StatusCode.Should().Be(StatusCodes.Status200OK);
        }

        [Fact]
        public async Task GetPatientCarePlans_WhenEmailDoesNotExists_ReturnsNotFound()
        {
            // Arrange
            var patientService = Substitute.For<IPatientService>();
            var alexaService = Substitute.For<IAlexaService>();
            var carePlanService = Substitute.For<ICarePlanService>();
            var observationService = Substitute.For<IObservationService>();
            var medicationRequestService = Substitute.For<IMedicationRequestService>();
            var logger = Substitute.For<ILogger<PatientController>>();
            var controller = new PatientController(patientService, alexaService, carePlanService, observationService,
                medicationRequestService, logger);
            carePlanService.GetCarePlanFor(Arg.Any<string>()).Throws(new KeyNotFoundException());

            // Act
            var carePlans = await controller.GetPatientCarePlans("john@mail.com");
            var result = (StatusCodeResult)carePlans;

            // Assert
            result.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        }

        [Fact]
        public async Task GetPatientCarePlans_WhenRequestFails_ReturnsInternalError()
        {
            // Arrange
            var patientService = Substitute.For<IPatientService>();
            var alexaService = Substitute.For<IAlexaService>();
            var carePlanService = Substitute.For<ICarePlanService>();
            var observationService = Substitute.For<IObservationService>();
            var medicationRequestService = Substitute.For<IMedicationRequestService>();
            var logger = Substitute.For<ILogger<PatientController>>();
            var controller = new PatientController(patientService, alexaService, carePlanService, observationService,
                medicationRequestService, logger);
            carePlanService.GetCarePlanFor(Arg.Any<string>()).Throws(new Exception());

            // Act
            var carePlans = await controller.GetPatientCarePlans("john@mail.com");
            var result = (StatusCodeResult)carePlans;

            // Assert
            result.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        }

        [Fact]
        public async Task GetActiveMedicationRequests_WhenRequestIsCorrect_ReturnsStatusOk()
        {
            // Arrange
            var patientService = Substitute.For<IPatientService>();
            var alexaService = Substitute.For<IAlexaService>();
            var carePlanService = Substitute.For<ICarePlanService>();
            var observationService = Substitute.For<IObservationService>();
            var medicationRequestService = Substitute.For<IMedicationRequestService>();
            var logger = Substitute.For<ILogger<PatientController>>();
            var controller = new PatientController(patientService, alexaService, carePlanService, observationService,
                medicationRequestService, logger);
            medicationRequestService.GetActiveMedicationRequests(Arg.Any<string>()).Returns(new Bundle());

            // Act
            var medicationRequests = await controller.GetActiveMedicationRequests("john@mail.com");
            var result = (ObjectResult)medicationRequests;

            // Assert
            result.StatusCode.Should().Be(StatusCodes.Status200OK);
        }

        [Fact]
        public async Task GetActiveMedicationRequests_WhenEmailDoesNotExists_ReturnsNotFound()
        {
            // Arrange
            var patientService = Substitute.For<IPatientService>();
            var alexaService = Substitute.For<IAlexaService>();
            var carePlanService = Substitute.For<ICarePlanService>();
            var observationService = Substitute.For<IObservationService>();
            var medicationRequestService = Substitute.For<IMedicationRequestService>();
            var logger = Substitute.For<ILogger<PatientController>>();
            var controller = new PatientController(patientService, alexaService, carePlanService, observationService,
                medicationRequestService, logger);
            medicationRequestService.GetActiveMedicationRequests(Arg.Any<string>()).Throws(new KeyNotFoundException());

            // Act
            var medicationRequests = await controller.GetActiveMedicationRequests("john@mail.com");
            var result = (StatusCodeResult)medicationRequests;

            // Assert
            result.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        }

        [Fact]
        public async Task GetActiveMedicationRequests_WhenRequestFails_ReturnsInternalError()
        {
            // Arrange
            var patientService = Substitute.For<IPatientService>();
            var alexaService = Substitute.For<IAlexaService>();
            var carePlanService = Substitute.For<ICarePlanService>();
            var observationService = Substitute.For<IObservationService>();
            var medicationRequestService = Substitute.For<IMedicationRequestService>();
            var logger = Substitute.For<ILogger<PatientController>>();
            var controller = new PatientController(patientService, alexaService, carePlanService, observationService,
                medicationRequestService, logger);
            medicationRequestService.GetActiveMedicationRequests(Arg.Any<string>()).Throws(new Exception());

            // Act
            var medicationRequests = await controller.GetActiveMedicationRequests("john@mail.com");
            var result = (StatusCodeResult)medicationRequests;

            // Assert
            result.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        }

        [Fact]
        public async Task GetActiveCarePlan_WhenRequestIsCorrect_ReturnsStatusOk()
        {
            // Arrange
            var patientService = Substitute.For<IPatientService>();
            var alexaService = Substitute.For<IAlexaService>();
            var carePlanService = Substitute.For<ICarePlanService>();
            var observationService = Substitute.For<IObservationService>();
            var medicationRequestService = Substitute.For<IMedicationRequestService>();
            var logger = Substitute.For<ILogger<PatientController>>();
            var controller = new PatientController(patientService, alexaService, carePlanService, observationService,
                medicationRequestService, logger);
            carePlanService.GetActiveCarePlans(Arg.Any<string>()).Returns(new Bundle());

            // Act
            var carePlan = await controller.GetActiveCarePlan("john@mail.com");
            var result = (ObjectResult)carePlan;

            // Assert
            result.StatusCode.Should().Be(StatusCodes.Status200OK);
        }

        [Fact]
        public async Task GetActiveCarePlan_WhenEmailIsNotFound_ReturnsNotFound()
        {
            // Arrange
            var patientService = Substitute.For<IPatientService>();
            var alexaService = Substitute.For<IAlexaService>();
            var carePlanService = Substitute.For<ICarePlanService>();
            var observationService = Substitute.For<IObservationService>();
            var medicationRequestService = Substitute.For<IMedicationRequestService>();
            var logger = Substitute.For<ILogger<PatientController>>();
            var controller = new PatientController(patientService, alexaService, carePlanService, observationService,
                medicationRequestService, logger);
            carePlanService.GetActiveCarePlans(Arg.Any<string>()).Throws(new KeyNotFoundException());

            // Act
            var carePlan = await controller.GetActiveCarePlan("john@mail.com");
            var result = (StatusCodeResult)carePlan;

            // Assert
            result.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        }

        [Fact]
        public async Task GetActiveCarePlan_WhenRequestFails_ReturnsInternalError()
        {
            // Arrange
            var patientService = Substitute.For<IPatientService>();
            var alexaService = Substitute.For<IAlexaService>();
            var carePlanService = Substitute.For<ICarePlanService>();
            var observationService = Substitute.For<IObservationService>();
            var medicationRequestService = Substitute.For<IMedicationRequestService>();
            var logger = Substitute.For<ILogger<PatientController>>();
            var controller = new PatientController(patientService, alexaService, carePlanService, observationService,
                medicationRequestService, logger);
            carePlanService.GetActiveCarePlans(Arg.Any<string>()).Throws(new Exception());

            // Act
            var carePlan = await controller.GetActiveCarePlan("john@mail.com");
            var result = (StatusCodeResult)carePlan;

            // Assert
            result.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        }

        [Fact]
        public async Task GetSingleObservation_WhenRequestIsCorrect_ReturnsStatusOk()
        {
            // Arrange
            var patientService = Substitute.For<IPatientService>();
            var alexaService = Substitute.For<IAlexaService>();
            var carePlanService = Substitute.For<ICarePlanService>();
            var observationService = Substitute.For<IObservationService>();
            var medicationRequestService = Substitute.For<IMedicationRequestService>();
            var logger = Substitute.For<ILogger<PatientController>>();
            var controller = new PatientController(patientService, alexaService, carePlanService, observationService,
                medicationRequestService, logger);
            observationService.GetSingleObservation(Arg.Any<string>()).Returns(new Observation());

            // Act
            var observation = await controller.GetSingleObservation("john@mail.com", Guid.NewGuid().ToString());
            var result = (ObjectResult)observation;

            // Assert
            result.StatusCode.Should().Be(StatusCodes.Status200OK);
        }

        [Fact]
        public async Task GetSingleObservation_WhenIdDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var patientService = Substitute.For<IPatientService>();
            var alexaService = Substitute.For<IAlexaService>();
            var carePlanService = Substitute.For<ICarePlanService>();
            var observationService = Substitute.For<IObservationService>();
            var medicationRequestService = Substitute.For<IMedicationRequestService>();
            var logger = Substitute.For<ILogger<PatientController>>();
            var controller = new PatientController(patientService, alexaService, carePlanService, observationService,
                medicationRequestService, logger);
            observationService.GetSingleObservation(Arg.Any<string>()).Throws(new KeyNotFoundException());

            // Act
            var observation = await controller.GetSingleObservation("john@mail.com", Guid.NewGuid().ToString());
            var result = (StatusCodeResult)observation;

            // Assert
            result.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        }

        [Fact]
        public async Task GetSingleObservation_WhenRequestFails_ReturnsInternalError()
        {
            // Arrange
            var patientService = Substitute.For<IPatientService>();
            var alexaService = Substitute.For<IAlexaService>();
            var carePlanService = Substitute.For<ICarePlanService>();
            var observationService = Substitute.For<IObservationService>();
            var medicationRequestService = Substitute.For<IMedicationRequestService>();
            var logger = Substitute.For<ILogger<PatientController>>();
            var controller = new PatientController(patientService, alexaService, carePlanService, observationService,
                medicationRequestService, logger);
            observationService.GetSingleObservation(Arg.Any<string>()).Throws(new Exception());

            // Act
            var observation = await controller.GetSingleObservation("john@mail.com", Guid.NewGuid().ToString());
            var result = (StatusCodeResult)observation;

            // Assert
            result.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        }

        [Fact]
        public async Task GetPatientObservations_WhenRequestIsCorrect_ReturnsStatusOk()
        {
            // Arrange
            var patientService = Substitute.For<IPatientService>();
            var alexaService = Substitute.For<IAlexaService>();
            var carePlanService = Substitute.For<ICarePlanService>();
            var observationService = Substitute.For<IObservationService>();
            var medicationRequestService = Substitute.For<IMedicationRequestService>();
            var logger = Substitute.For<ILogger<PatientController>>();
            var controller = new PatientController(patientService, alexaService, carePlanService, observationService,
                medicationRequestService, logger);
            observationService.GetObservationsFor(Arg.Any<string>(), Arg.Any<CustomEventTiming>(), Arg.Any<DateTime>(), Arg.Any<string>())
                .Returns(new Bundle());

            // Act
            var observations = await controller.GetPatientObservations("john@mail.com", DateTime.Now);
            var result = (ObjectResult)observations;

            // Assert
            result.StatusCode.Should().Be(StatusCodes.Status200OK);
        }

        [Fact]
        public async Task GetPatientObservations_WhenEmailDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var patientService = Substitute.For<IPatientService>();
            var alexaService = Substitute.For<IAlexaService>();
            var carePlanService = Substitute.For<ICarePlanService>();
            var observationService = Substitute.For<IObservationService>();
            var medicationRequestService = Substitute.For<IMedicationRequestService>();
            var logger = Substitute.For<ILogger<PatientController>>();
            var controller = new PatientController(patientService, alexaService, carePlanService, observationService,
                medicationRequestService, logger);
            observationService.GetObservationsFor(Arg.Any<string>(), Arg.Any<CustomEventTiming>(), Arg.Any<DateTime>(), Arg.Any<string>())
                .Throws(new KeyNotFoundException());

            // Act
            var observations = await controller.GetPatientObservations("john@mail.com", DateTime.Now);
            var result = (StatusCodeResult)observations;

            // Assert
            result.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        }

        [Fact]
        public async Task GetPatientObservations_WhenRequestFails_ReturnsInternalError()
        {
            // Arrange
            var patientService = Substitute.For<IPatientService>();
            var alexaService = Substitute.For<IAlexaService>();
            var carePlanService = Substitute.For<ICarePlanService>();
            var observationService = Substitute.For<IObservationService>();
            var medicationRequestService = Substitute.For<IMedicationRequestService>();
            var logger = Substitute.For<ILogger<PatientController>>();
            var controller = new PatientController(patientService, alexaService, carePlanService, observationService,
                medicationRequestService, logger);
            observationService.GetObservationsFor(Arg.Any<string>(), Arg.Any<CustomEventTiming>(), Arg.Any<DateTime>(), Arg.Any<string>())
                .Throws(new Exception());

            // Act
            var observations = await controller.GetPatientObservations("john@mail.com", DateTime.Now);
            var result = (StatusCodeResult)observations;

            // Assert
            result.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        }

        [Fact]
        public async Task GetAllPatientObservations_WhenRequestIsCorrect_ReturnsStatusOk()
        {
            // Arrange
            var patientService = Substitute.For<IPatientService>();
            var alexaService = Substitute.For<IAlexaService>();
            var carePlanService = Substitute.For<ICarePlanService>();
            var observationService = Substitute.For<IObservationService>();
            var medicationRequestService = Substitute.For<IMedicationRequestService>();
            var logger = Substitute.For<ILogger<PatientController>>();
            var controller = new PatientController(patientService, alexaService, carePlanService, observationService,
                medicationRequestService, logger);
            observationService.GetAllObservationsFor(Arg.Any<string>()).Returns(new Bundle());

            // Act
            var observations = await controller.GetAllPatientObservations("john@mail.com");
            var result = (ObjectResult)observations;

            // Assert
            result.StatusCode.Should().Be(StatusCodes.Status200OK);
        }

        [Fact]
        public async Task GetAllPatientObservations_WhenEmailDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var patientService = Substitute.For<IPatientService>();
            var alexaService = Substitute.For<IAlexaService>();
            var carePlanService = Substitute.For<ICarePlanService>();
            var observationService = Substitute.For<IObservationService>();
            var medicationRequestService = Substitute.For<IMedicationRequestService>();
            var logger = Substitute.For<ILogger<PatientController>>();
            var controller = new PatientController(patientService, alexaService, carePlanService, observationService,
                medicationRequestService, logger);
            observationService.GetAllObservationsFor(Arg.Any<string>()).Throws(new KeyNotFoundException());

            // Act
            var observations = await controller.GetAllPatientObservations("john@mail.com");
            var result = (StatusCodeResult)observations;

            // Assert
            result.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        }

        [Fact]
        public async Task GetAllPatientObservations_WhenRequestFails_ReturnsInternalError()
        {
            // Arrange
            var patientService = Substitute.For<IPatientService>();
            var alexaService = Substitute.For<IAlexaService>();
            var carePlanService = Substitute.For<ICarePlanService>();
            var observationService = Substitute.For<IObservationService>();
            var medicationRequestService = Substitute.For<IMedicationRequestService>();
            var logger = Substitute.For<ILogger<PatientController>>();
            var controller = new PatientController(patientService, alexaService, carePlanService, observationService,
                medicationRequestService, logger);
            observationService.GetAllObservationsFor(Arg.Any<string>()).Throws(new Exception());

            // Act
            var observations = await controller.GetAllPatientObservations("john@mail.com");
            var result = (StatusCodeResult)observations;

            // Assert
            result.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        }

        [Fact]
        public async Task UpdatePatientTiming_WhenRequestIsCorrect_ReturnsNoContent()
        {
            // Arrange
            var patientService = Substitute.For<IPatientService>();
            var alexaService = Substitute.For<IAlexaService>();
            var carePlanService = Substitute.For<ICarePlanService>();
            var observationService = Substitute.For<IObservationService>();
            var medicationRequestService = Substitute.For<IMedicationRequestService>();
            var logger = Substitute.For<ILogger<PatientController>>();
            var controller = new PatientController(patientService, alexaService, carePlanService, observationService,
                medicationRequestService, logger);
            alexaService.UpsertTimingEvent(Arg.Any<string>(), Arg.Any<CustomEventTiming>(), Arg.Any<DateTime>())
                .Returns(true);

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
            var patientService = Substitute.For<IPatientService>();
            var alexaService = Substitute.For<IAlexaService>();
            var carePlanService = Substitute.For<ICarePlanService>();
            var observationService = Substitute.For<IObservationService>();
            var medicationRequestService = Substitute.For<IMedicationRequestService>();
            var logger = Substitute.For<ILogger<PatientController>>();
            var controller = new PatientController(patientService, alexaService, carePlanService, observationService,
                medicationRequestService, logger);
            alexaService.UpsertTimingEvent(Arg.Any<string>(), Arg.Any<CustomEventTiming>(), Arg.Any<DateTime>())
                .Returns(false);

            // Act
            var updated = await controller.UpdatePatientTiming("john@mail.com", new PatientTimingRequest());
            var result = (StatusCodeResult)updated;

            // Assert
            result.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        }

        [Fact]
        public async Task UpdatePatientTiming_WhenEmailDoesNotExists_ReturnsBadRequest()
        {
            // Arrange
            var patientService = Substitute.For<IPatientService>();
            var alexaService = Substitute.For<IAlexaService>();
            var carePlanService = Substitute.For<ICarePlanService>();
            var observationService = Substitute.For<IObservationService>();
            var medicationRequestService = Substitute.For<IMedicationRequestService>();
            var logger = Substitute.For<ILogger<PatientController>>();
            var controller = new PatientController(patientService, alexaService, carePlanService, observationService,
                medicationRequestService, logger);
            alexaService.UpsertTimingEvent(Arg.Any<string>(), Arg.Any<CustomEventTiming>(), Arg.Any<DateTime>())
                .Throws(new KeyNotFoundException());

            // Act
            var updated = await controller.UpdatePatientTiming("john@mail.com", new PatientTimingRequest());
            var result = (StatusCodeResult)updated;

            // Assert
            result.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        }

        [Fact]
        public async Task UpdatePatientTiming_WhenRequestFails_ReturnsInternalError()
        {
            // Arrange
            var patientService = Substitute.For<IPatientService>();
            var alexaService = Substitute.For<IAlexaService>();
            var carePlanService = Substitute.For<ICarePlanService>();
            var observationService = Substitute.For<IObservationService>();
            var medicationRequestService = Substitute.For<IMedicationRequestService>();
            var logger = Substitute.For<ILogger<PatientController>>();
            var controller = new PatientController(patientService, alexaService, carePlanService, observationService,
                medicationRequestService, logger);
            alexaService.UpsertTimingEvent(Arg.Any<string>(), Arg.Any<CustomEventTiming>(), Arg.Any<DateTime>())
                .Throws(new Exception());

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
            var patientService = Substitute.For<IPatientService>();
            var alexaService = Substitute.For<IAlexaService>();
            var carePlanService = Substitute.For<ICarePlanService>();
            var observationService = Substitute.For<IObservationService>();
            var medicationRequestService = Substitute.For<IMedicationRequestService>();
            var logger = Substitute.For<ILogger<PatientController>>();
            var controller = new PatientController(patientService, alexaService, carePlanService, observationService,
                medicationRequestService, logger);
            alexaService.UpsertDosageStartDate(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<DateTime>())
                .Returns(true);

            // Act
            var updated = await controller.UpdateDosageStartDate("john@mail.com", Guid.NewGuid().ToString(),
                new PatientStartDateRequest());
            var result = (StatusCodeResult)updated;

            // Assert
            result.StatusCode.Should().Be(StatusCodes.Status204NoContent);
        }

        [Fact]
        public async Task UpdateDosageStartDate_WhenDoesNotUpdate_ReturnsBadRequest()
        {
            // Arrange
            var patientService = Substitute.For<IPatientService>();
            var alexaService = Substitute.For<IAlexaService>();
            var carePlanService = Substitute.For<ICarePlanService>();
            var observationService = Substitute.For<IObservationService>();
            var medicationRequestService = Substitute.For<IMedicationRequestService>();
            var logger = Substitute.For<ILogger<PatientController>>();
            var controller = new PatientController(patientService, alexaService, carePlanService, observationService,
                medicationRequestService, logger);
            alexaService.UpsertDosageStartDate(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<DateTime>())
                .Returns(false);

            // Act
            var updated = await controller.UpdateDosageStartDate("john@mail.com", Guid.NewGuid().ToString(),
                new PatientStartDateRequest());
            var result = (StatusCodeResult)updated;

            // Assert
            result.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        }

        [Fact]
        public async Task UpdateDosageStartDate_WhenEmailDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var patientService = Substitute.For<IPatientService>();
            var alexaService = Substitute.For<IAlexaService>();
            var carePlanService = Substitute.For<ICarePlanService>();
            var observationService = Substitute.For<IObservationService>();
            var medicationRequestService = Substitute.For<IMedicationRequestService>();
            var logger = Substitute.For<ILogger<PatientController>>();
            var controller = new PatientController(patientService, alexaService, carePlanService, observationService,
                medicationRequestService, logger);
            alexaService.UpsertDosageStartDate(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<DateTime>())
                .Throws(new KeyNotFoundException());

            // Act
            var updated = await controller.UpdateDosageStartDate("john@mail.com", Guid.NewGuid().ToString(),
                new PatientStartDateRequest());
            var result = (StatusCodeResult)updated;

            // Assert
            result.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        }

        [Fact]
        public async Task UpdateDosageStartDate_WhenRequestFails_ReturnsInternalError()
        {
            // Arrange
            var patientService = Substitute.For<IPatientService>();
            var alexaService = Substitute.For<IAlexaService>();
            var carePlanService = Substitute.For<ICarePlanService>();
            var observationService = Substitute.For<IObservationService>();
            var medicationRequestService = Substitute.For<IMedicationRequestService>();
            var logger = Substitute.For<ILogger<PatientController>>();
            var controller = new PatientController(patientService, alexaService, carePlanService, observationService,
                medicationRequestService, logger);
            alexaService.UpsertDosageStartDate(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<DateTime>())
                .Throws(new Exception());

            // Act
            var updated = await controller.UpdateDosageStartDate("john@mail.com", Guid.NewGuid().ToString(),
                new PatientStartDateRequest());
            var result = (StatusCodeResult)updated;

            // Assert
            result.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        }
    }
}