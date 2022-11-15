namespace QMUL.DiabetesBackend.Controllers.Tests.Utils
{
    using System;
    using System.Threading.Tasks;
    using Api.Utils;
    using FluentAssertions;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Model.Exceptions;
    using NSubstitute;
    using Xunit;

    public class ExceptionHandlerTest
    {
        [Fact]
        public async Task ExecuteAndHandleAsync_WhenMethodDoesNotThrow_ReturnResult()
        {
            // Arrange
            var controller = Substitute.For<ControllerBase>();
            controller.Ok(Arg.Any<object>()).Returns(new OkObjectResult(new object()));
            var logger = Substitute.For<ILogger>();
            var method = new Func<Task<OkObjectResult>>(() => Task.FromResult(controller.Ok(new object())));

            // Act
            var result = await ExceptionHandler.ExecuteAndHandleAsync(method, logger, controller);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task ExecuteAndHandleAsync_WhenMethodThrowsNotFound_ReturnsNotFound()
        {
            // Arrange
            var controller = Substitute.For<ControllerBase>();
            controller.NotFound().Returns(new NotFoundResult());
            var logger = Substitute.For<ILogger>();
            var method = new Func<Task<IActionResult>>(() => throw new NotFoundException(string.Empty));

            // Act
            var result = await ExceptionHandler.ExecuteAndHandleAsync(method, logger, controller);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task ExecuteAndHandleAsync_WhenMethodThrowsCreateException_ReturnsBadRequest()
        {
            // Arrange
            var controller = Substitute.For<ControllerBase>();
            controller.BadRequest().Returns(new BadRequestResult());
            var logger = Substitute.For<ILogger>();
            var method = new Func<Task<IActionResult>>(() => throw new WriteResourceException(string.Empty));

            // Act
            var result = await ExceptionHandler.ExecuteAndHandleAsync(method, logger, controller);

            // Assert
            result.Should().BeOfType<BadRequestResult>();
        }

        [Fact]
        public async Task ExecuteAndHandleAsync_WhenMethodThrowsValidationException_ReturnsBadRequest()
        {
            // Arrange
            var controller = Substitute.For<ControllerBase>();
            controller.BadRequest().Returns(new BadRequestResult());
            var logger = Substitute.For<ILogger>();
            var method = new Func<Task<IActionResult>>(() => throw new ValidationException(string.Empty));

            // Act
            var result = await ExceptionHandler.ExecuteAndHandleAsync(method, logger, controller);

            // Assert
            result.Should().BeOfType<BadRequestResult>();
        }

        [Fact]
        public async Task ExecuteAndHandleAsync_WhenMethodThrowsArgumentException_ReturnsBadRequest()
        {
            // Arrange
            var controller = Substitute.For<ControllerBase>();
            controller.BadRequest().Returns(new BadRequestResult());
            var logger = Substitute.For<ILogger>();
            var method = new Func<Task<IActionResult>>(() => throw new ArgumentException());

            // Act
            var result = await ExceptionHandler.ExecuteAndHandleAsync(method, logger, controller);

            // Assert
            result.Should().BeOfType<BadRequestResult>();
        }

        [Fact]
        public async Task ExecuteAndHandleAsync_WhenMethodThrowsException_ReturnsInternalError()
        {
            // Arrange
            var controller = Substitute.For<ControllerBase>();
            controller.StatusCode(StatusCodes.Status500InternalServerError)
                .Returns(new StatusCodeResult(StatusCodes.Status500InternalServerError));
            var logger = Substitute.For<ILogger>();
            var method = new Func<Task<IActionResult>>(() => throw new Exception());

            // Act
            var result = await ExceptionHandler.ExecuteAndHandleAsync(method, logger, controller);

            // Assert
            result.Should().BeOfType<StatusCodeResult>();
            controller.Received(1).StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}