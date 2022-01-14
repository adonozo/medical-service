namespace QMUL.DiabetesBackend.ServiceImpl.Tests.Utils
{
    using System;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Microsoft.Extensions.Logging;
    using NSubstitute;
    using ServiceImpl.Utils;
    using Xunit;

    public class ExceptionHandlerTest
    {
        [Fact]
        public async Task ExecuteAndHandleAsync_WhenMethodDoesNotThrow_ReturnsResult()
        {
            // Arrange
            const string expectedResult = "success";
            var logger = Substitute.For<ILogger>();
            var function = new Func<Task<string>>(() => Task.FromResult(expectedResult));

            // Act
            var result = await ExceptionHandler.ExecuteAndHandleAsync(function, logger);

            // Assert
            result.Should().Be(expectedResult);
        }
        
        [Fact]
        public async Task ExecuteAndHandleAsync_WhenMethodThrowsCreateException_ThrowsException()
        {
            // Arrange
            var logger = Substitute.For<ILogger>();
            var function = new Func<Task<string>>(() => throw new DataInterfaces.Exceptions.CreateException(string.Empty));

            // Act
            var action = new Func<Task>(() => ExceptionHandler.ExecuteAndHandleAsync(function, logger));

            // Assert
            await action.Should().ThrowAsync<ServiceInterfaces.Exceptions.CreateException>();
        }
        
        [Fact]
        public async Task ExecuteAndHandleAsync_WhenMethodThrowsNotFoundException_ThrowsException()
        {
            // Arrange
            var logger = Substitute.For<ILogger>();
            var function = new Func<Task<string>>(() => throw new DataInterfaces.Exceptions.NotFoundException(string.Empty));

            // Act
            var action = new Func<Task>(() => ExceptionHandler.ExecuteAndHandleAsync(function, logger));

            // Assert
            await action.Should().ThrowAsync<ServiceInterfaces.Exceptions.NotFoundException>();
        }
        
        [Fact]
        public async Task ExecuteAndHandleAsync_WhenMethodThrowsUpdateException_ThrowsException()
        {
            // Arrange
            var logger = Substitute.For<ILogger>();
            var function = new Func<Task<string>>(() => throw new DataInterfaces.Exceptions.UpdateException(string.Empty));

            // Act
            var action = new Func<Task>(() => ExceptionHandler.ExecuteAndHandleAsync(function, logger));

            // Assert
            await action.Should().ThrowAsync<ServiceInterfaces.Exceptions.UpdateException>();
        }
        
        [Fact]
        public async Task ExecuteAndHandleAsync_WhenMethodThrowsException_ThrowsSame()
        {
            // Arrange
            var logger = Substitute.For<ILogger>();
            var function = new Func<Task<string>>(() => throw new ArgumentException());

            // Act
            var action = new Func<Task>(() => ExceptionHandler.ExecuteAndHandleAsync(function, logger));

            // Assert
            await action.Should().ThrowAsync<ArgumentException>();
        }
    }
}