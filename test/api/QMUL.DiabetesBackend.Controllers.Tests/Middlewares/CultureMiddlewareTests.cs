namespace QMUL.DiabetesBackend.Controllers.Tests.Middlewares;

using System.Threading.Tasks;
using DiabetesBackend.Controllers.Middlewares;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Xunit;

public class CultureMiddlewareTests
{
    [Theory]
    [InlineData("es", "es")]
    [InlineData("es-MX", "es")]
    [InlineData("en", "en")]
    [InlineData("en-GB", "en")]
    public async Task GivenAnAcceptLanguageHeader_WhenCultureIsSupported_SetsCulture(string providedCulture,
        string expectedCulture)
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers.Add("Accept-Language", providedCulture);
        var middleware = new CultureMiddleware(_ => Task.CompletedTask);

        // Act
        await middleware.InvokeAsync(httpContext);

        // Assert
        httpContext.Response.Headers.Should().ContainKey("Accept-Language")
            .WhoseValue.Should().BeEquivalentTo(expectedCulture);
    }

    [Theory]
    [InlineData("")]
    [InlineData("fr")]
    [InlineData("fr-CA")]
    [InlineData("ge")]
    public async Task GivenAnAcceptLanguageHeader_WhenCultureIsNotSupported_SetsDefaultCulture(string providedCulture)
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers.Add("Accept-Language", providedCulture);
        var middleware = new CultureMiddleware(_ => Task.CompletedTask);

        // Act
        await middleware.InvokeAsync(httpContext);

        // Assert
        httpContext.Response.Headers.Should().ContainKey("Accept-Language")
            .WhoseValue.Should().BeEquivalentTo(CultureMiddleware.DefaultCulture);
    }
}
