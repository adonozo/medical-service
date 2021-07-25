using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using QMUL.DiabetesBackend.Api.Controllers;
using QMUL.DiabetesBackend.ServiceInterfaces;
using System.Collections.Generic;
using Xunit;

namespace QMUL.DiabetesBackend.Controllers.Tests
{
    public class CarePlanControllerTest
    {
        [Fact]
        public void GetCarePlan_ValidId_ReturnsStatusOk()
        {
            //Arrage
            var logger = Substitute.For<ILogger<CarePlanController>>();
            var carePlanService = Substitute.For<ICarePlanService>();
            carePlanService.GetCarePlan(Arg.Any<string>()).Returns(new Hl7.Fhir.Model.CarePlan());
            var medicationController = new CarePlanController(carePlanService, logger);

            //Act
            var result = medicationController.GetCarePlan("Test");
            var statusCode = ((ObjectResult)result).StatusCode;

            //Assert
            statusCode.Should().Be(StatusCodes.Status200OK);
        }

        [Fact]
        public void GetCarePlan_InvalidId_ReturnsNotFound()
        {
            //Arrage
            var logger = Substitute.For<ILogger<CarePlanController>>();
            var carePlanService = Substitute.For<ICarePlanService>();
            carePlanService.GetCarePlan(Arg.Any<string>()).Returns(value => throw new KeyNotFoundException());
            var medicationController = new CarePlanController(carePlanService, logger);

            //Act
            var result = medicationController.GetCarePlan(string.Empty);
            var statusCode = ((StatusCodeResult)result).StatusCode;

            //Assert
            statusCode.Should().Be(StatusCodes.Status404NotFound);
        }
    }
}
