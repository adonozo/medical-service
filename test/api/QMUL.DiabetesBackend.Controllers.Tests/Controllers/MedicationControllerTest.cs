namespace QMUL.DiabetesBackend.Controllers.Tests.Controllers
{
    using System;
    using FluentAssertions;
    using Hl7.Fhir.Model;
    using Hl7.Fhir.Serialization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Model;
    using Newtonsoft.Json.Linq;
    using NSubstitute;
    using NSubstitute.ExceptionExtensions;
    using Api.Controllers;
    using Model.Exceptions;
    using ServiceInterfaces;
    using ServiceInterfaces.Validators;
    using Xunit;
    using Task = System.Threading.Tasks.Task;

    public class MedicationControllerTest
    {
        [Fact]
        public async Task GetAllMedications_WhenRequestIsCorrect_ReturnsOk()
        {
            // Arrange
            var paginatedResult = new PaginatedResult<Bundle>
            {
                Results = new Bundle()
            };
            var service = Substitute.For<IMedicationService>();
            service.GetMedicationList(Arg.Any<PaginationRequest>()).Returns(paginatedResult);

            var controller = this.GetMedicationController(service);

            // Act
            var medications = await controller.GetAllMedications();
            var result = (ObjectResult)medications;

            // Assert
            result.StatusCode.Should().Be(StatusCodes.Status200OK);
        }

        [Fact]
        public async Task GetAllMedications_WhenRequestFails_ReturnsInternalError()
        {
            // Arrange
            var service = Substitute.For<IMedicationService>();
            service.GetMedicationList(Arg.Any<PaginationRequest>()).Throws(new Exception());

            var controller = this.GetMedicationController(service);

            // Act
            var medications = await controller.GetAllMedications();
            var result = (StatusCodeResult)medications;

            // Assert
            result.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        }

        [Fact]
        public async Task GetMedication_WhenRequestIsCorrect_ReturnsOk()
        {
            // Arrange
            var service = Substitute.For<IMedicationService>();
            var id = Guid.NewGuid().ToString();
            service.GetMedication(Arg.Any<string>()).Returns(new Medication());

            var controller = this.GetMedicationController(service);

            // Act
            var medication = await controller.GetMedication(id);
            var result = (ObjectResult)medication;

            // Assert
            result.StatusCode.Should().Be(StatusCodes.Status200OK);
        }

        [Fact]
        public async Task GetMedication_WhenRequestFails_ReturnsInternalError()
        {
            // Arrange
            var service = Substitute.For<IMedicationService>();
            var id = Guid.NewGuid().ToString();
            service.GetMedication(Arg.Any<string>()).Throws(new Exception());

            var controller = this.GetMedicationController(service);

            // Act
            var medicationResult = await controller.GetMedication(id);
            var result = (StatusCodeResult)medicationResult;

            // Assert
            result.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        }

        [Fact]
        public async Task CreateMedication_WhenRequestIsCorrect_ReturnsOk()
        {
            // Arrange
            var service = Substitute.For<IMedicationService>();
            var medication = this.GetTestMedication(Guid.NewGuid().ToString());
            service.CreateMedication(Arg.Any<Medication>()).Returns(medication);

            var controller = this.GetMedicationController(service);

            // Act
            var medicationCreated = await controller.CreateMedication(medication.ToJObject());
            var result = (ObjectResult)medicationCreated;

            // Assert
            result.StatusCode.Should().Be(StatusCodes.Status200OK);
        }

        [Fact]
        public async Task CreateMedication_WhenRequestUnformatted_ReturnsBadRequest()
        {
            // Arrange
            var service = Substitute.For<IMedicationService>();
            var unformattedObject = new InternalPatient();
            var validator = Substitute.For<IResourceValidator<Medication>>();
            validator.ParseAndValidateAsync(Arg.Any<JObject>()).Throws(new ValidationException(string.Empty));

            var controller = this.GetMedicationController(service, validator);

            // Act
            var createdResult = await controller.CreateMedication(JObject.FromObject(unformattedObject));
            var result = (ObjectResult)createdResult;

            // Assert
            result.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        }

        [Fact]
        public async Task CreateMedication_WhenRequestFails_ReturnsInternalError()
        {
            // Arrange
            var service = Substitute.For<IMedicationService>();
            service.CreateMedication(Arg.Any<Medication>()).Throws(new Exception());
            var medication = this.GetTestMedication(Guid.NewGuid().ToString());

            var controller = this.GetMedicationController(service);

            // Act
            var medicationCreated = await controller.CreateMedication(medication.ToJObject());
            var result = (StatusCodeResult)medicationCreated;

            // Assert
            result.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        }

        #region Private methods

        private Medication GetTestMedication(string id)
        {
            return new Medication
            {
                Id = id,
                Code = new CodeableConcept(),
                Status = Medication.MedicationStatusCodes.Active
            };
        }

        private MedicationController GetMedicationController(IMedicationService service,
            IResourceValidator<Medication> validator = null)
        {
            var logger = Substitute.For<ILogger<MedicationController>>();
            validator ??= Substitute.For<IResourceValidator<Medication>>();

            return new MedicationController(service, validator, logger)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext(),
                }
            };
        }

        #endregion
    }
}