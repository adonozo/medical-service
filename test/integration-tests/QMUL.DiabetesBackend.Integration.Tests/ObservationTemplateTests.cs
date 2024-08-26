namespace QMUL.DiabetesBackend.Integration.Tests;

using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Model;
using Stubs;
using Utils;
using Xunit;

[Collection(TestFixture.IntegrationTestCollection)]
public class ObservationTemplateTests : IntegrationTestBase
{
    public ObservationTemplateTests(TestFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task GetObservationTemplates_ReturnsOk()
    {
        // Arrange & Act
        var response = await this.HttpClient.GetAsync("observation-templates");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateObservationTemplate_ReturnsCreatedTemplate()
    {
        // Arrange
        var template = ObservationTemplateStubs.GlucoseTemplate;

        // Act
        var createResponse = await this.HttpClient.PostJson("observation-templates", template);

        // Assert
        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var createdTemplate = await createResponse.Content.Parse<ObservationTemplate>();
        createdTemplate.Id.Should().NotBeNull();

        var getResponse = await this.HttpClient.GetAsync($"observation-templates/{createdTemplate.Id}");
        var savedTemplate = await getResponse.Content.Parse<ObservationTemplate>();
        savedTemplate.Should().BeEquivalentTo(createdTemplate);
    }
}